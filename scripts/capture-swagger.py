#!/usr/bin/env python3
"""Execute each endpoint in Swagger UI and capture actual response screenshots."""
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess

from playwright.sync_api import sync_playwright

SPEC = importlib.util.spec_from_file_location('verify_api', Path(__file__).with_name('verify-api.py'))
api = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(api)
OUTPUT = api.ROOT / 'docs/evidencias'


class SwaggerClient(api.HttpClient):
    def __init__(self, url, page):
        super().__init__(url)
        self.page = page
        self.captured = set()
        self.records = []

    def request(self, method, path, expected, body=None, label=None):
        template = api.route_template(path)
        key = (method, template, expected)
        positive = expected < 400
        selected_error = (expected == 400 and method == 'POST' and template == '/api/v1/livros') or (
            expected == 404 and method == 'GET' and template == '/api/v1/livros/{id}') or (
            expected == 400 and method == 'PUT' and '/{usuarioId}/livros/' in template)
        if key in self.captured or not (positive or selected_error) or '/api/v2/' in path:
            return super().request(method, path, expected, body, label)
        page = self.page
        page.goto(self.url + '/swagger/index.html')
        page.locator('.opblock').first.wait_for()
        block = page.locator(f'.opblock.opblock-{method.lower()}').filter(
            has=page.locator(f'[data-path="{template}"]'))
        assert block.count() == 1, f'Operação Swagger ausente: {method} {template}'
        block.locator('.opblock-summary-control').click()
        block.get_by_role('button', name='Try it out').click()
        actual_parts, template_parts = path.split('/'), template.split('/')
        for name, value in zip(template_parts, actual_parts):
            if name.startswith('{'):
                parameter = name[1:-1]
                block.locator(f'tr[data-param-name="{parameter}"] input').fill(value)
        if body is not None:
            block.locator('textarea.body-param__text').fill(json.dumps(body, ensure_ascii=False, indent=2))
        with page.expect_response(lambda response: response.url == self.url + path and response.request.method == method) as pending:
            block.get_by_role('button', name='Execute', exact=True).click()
        response = pending.value
        raw = response.body()
        payload = json.loads(raw) if raw else None
        assert response.status == expected, f'Swagger {method} {path}: {response.status}, esperado {expected}: {payload}'
        if expected == 201:
            assert response.headers.get('location')
        if expected == 204:
            assert not raw
        if expected >= 400:
            assert payload['status'] == expected
        block.locator('.live-responses-table').wait_for()
        status_text = block.locator(".live-responses-table .response-col_status:not(.col_header)").inner_text()
        assert str(expected) in status_text, status_text
        slug = re.sub(r'[^a-z0-9]+', '-', template.lower()).strip('-')
        filename = f'{len(self.records) + 1:02d}-{method.lower()}-{slug}-{expected}.png'
        block.screenshot(path=str(OUTPUT / filename), animations='disabled')
        self.records.append({'method': method, 'route': template, 'path': path, 'status': expected, 'image': filename})
        self.captured.add(key)
        self.count += 1
        self.operations.add((method, template))
        print(f'SWAGGER {method} {path} -> {expected} [{filename}]', flush=True)
        return payload


def main():
    subprocess.run(['dotnet', 'build', '--nologo'], cwd=api.ROOT, check=True)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    with api.ApiProcess() as server, sync_playwright() as playwright:
        browser = playwright.chromium.launch(executable_path=os.environ.get('CHROMIUM_PATH', '/usr/bin/chromium'),
                                             headless=True, args=['--no-sandbox'])
        try:
            page = browser.new_page(viewport={'width': 1440, 'height': 1100}, device_scale_factor=1)
            page.set_default_timeout(15000)
            client = SwaggerClient(server.url, page)
            api.verify(client)
            document = json.loads(page.request.get(server.url + '/swagger/v1/swagger.json').text())
            advertised = {(m.upper(), p) for p, item in document['paths'].items() for m in item
                          if m in {'get', 'post', 'put', 'delete'}}
            captured = {(r['method'], r['route']) for r in client.records if r['status'] < 400}
            assert advertised <= captured, f'Faltam prints: {advertised - captured}'
            (OUTPUT / 'manifest.json').write_text(json.dumps(client.records, ensure_ascii=False, indent=2) + '\n')
            lines = ['# Evidências do Swagger', '',
                     'Capturas reais de requisições executadas pelo botão Execute do Swagger UI, contra a API .NET 10 e uma base SQLite temporária com a migration aplicada.', '',
                     'Cada imagem contém a operação, os parâmetros e a resposta HTTP. O teste confere o status e a cobertura de todas as operações descritas no OpenAPI.', '',
                     '| Método | Rota | Status | Print |', '|---|---|---|---|']
            lines.extend(f"| {r['method']} | `{r['route']}` | {r['status']} | [Abrir]({r['image']}) |" for r in client.records)
            lines += ['', '## Reproduzir', '', 'Instale Playwright em um ambiente Python e execute na raiz do repositório:', '',
                      '```bash', 'python -m venv .venv', 'source .venv/bin/activate',
                      'pip install -r scripts/requirements-evidence.txt',
                      'python scripts/capture-swagger.py', '```', '',
                      'O script usa Chromium instalado em `/usr/bin/chromium`. Defina `CHROMIUM_PATH` para usar outro caminho. Ele cria e apaga seu próprio banco temporário.', '']
            (OUTPUT / 'README.md').write_text('\n'.join(lines))
            print(f'{len(client.records)} prints reais; {len(advertised)} operações cobertas.')
        finally:
            browser.close()


if __name__ == '__main__':
    main()
