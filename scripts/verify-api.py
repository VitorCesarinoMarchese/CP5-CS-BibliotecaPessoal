#!/usr/bin/env python3
"""Run the real API against a temporary SQLite database and verify its HTTP contract."""
import json
import os
from pathlib import Path
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / 'src/BibliotecaPessoal.Api'


class ApiProcess:
    def __enter__(self):
        self.temp = tempfile.TemporaryDirectory(prefix='biblioteca-verificacao-')
        self.database = Path(self.temp.name) / 'verificacao.db'
        with socket.socket() as sock:
            sock.bind(('127.0.0.1', 0))
            self.port = sock.getsockname()[1]
        self.url = f'http://127.0.0.1:{self.port}'
        self.env = {**os.environ, 'ASPNETCORE_ENVIRONMENT': 'Development',
                    'ConnectionStrings__Biblioteca': f'Data Source={self.database};Foreign Keys=True'}
        self.process = None
        self.log = open(Path(self.temp.name) / 'api.log', 'w+')
        try:
            subprocess.run(['dotnet', 'ef', 'database', 'update', '--project', str(PROJECT), '--no-build'],
                           cwd=ROOT, env=self.env, check=True, stdout=self.log, stderr=self.log)
            self.start()
            return self
        except BaseException:
            self.__exit__(None, None, None)
            raise

    def start(self):
        self.process = subprocess.Popen(['dotnet', str(PROJECT / 'bin/Debug/net10.0/BibliotecaPessoal.Api.dll'),
                                         '--urls', self.url], cwd=PROJECT, env=self.env,
                                        stdout=self.log, stderr=self.log)
        deadline = time.monotonic() + 30
        while time.monotonic() < deadline:
            if self.process.poll() is not None:
                self.log.seek(0)
                raise RuntimeError(self.log.read())
            try:
                urllib.request.urlopen(self.url + '/swagger/v1/swagger.json', timeout=1).close()
                return
            except (urllib.error.URLError, TimeoutError):
                time.sleep(0.1)
        raise RuntimeError('A API não iniciou em 30 segundos.')

    def stop(self):
        if self.process and self.process.poll() is None:
            self.process.terminate()
            try:
                self.process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                self.process.kill()
                self.process.wait()

    def restart(self):
        self.stop()
        self.start()

    def __exit__(self, *args):
        self.stop()
        self.log.close()
        self.temp.cleanup()


class HttpClient:
    def __init__(self, url):
        self.url = url
        self.count = 0
        self.operations = set()

    def request(self, method, path, expected, body=None, label=None):
        data = None if body is None else json.dumps(body).encode()
        request = urllib.request.Request(self.url + path, data=data, method=method,
                                         headers={'Content-Type': 'application/json'})
        try:
            response = urllib.request.urlopen(request, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            status = response.status
            raw = response.read()
            headers = response.headers
        payload = json.loads(raw) if raw else None
        assert status == expected, f'{method} {path}: esperado {expected}, recebido {status}: {payload}'
        if expected == 201:
            assert headers.get('Location'), f'{path}: Location ausente'
        if expected == 204:
            assert not raw, f'{path}: 204 deve ter corpo vazio'
        if expected >= 400:
            assert payload['status'] == expected, f'{path}: ProblemDetails ausente'
        self.count += 1
        self.operations.add((method, route_template(path)))
        print(f'OK {method} {path} -> {status}', flush=True)
        return payload


def route_template(path):
    parts = path.split('/')
    if len(parts) > 4:
        parts[4] = '{usuarioId}' if parts[3] == 'usuarios' and len(parts) > 5 else '{id}'
    if len(parts) > 6:
        parts[6] = '{livroId}'
    return '/'.join(parts)


def verify(client):
    call = client.request
    refs = {}
    for resource, name in [('autores', 'Machado de Assis'), ('generos', 'Romance')]:
        base = f'/api/v1/{resource}'
        created = call('POST', base, 201, {'nome': name})
        refs[resource] = created['id']
        assert call('GET', f"{base}/{created['id']}", 200)['nome'] == name
        assert any(x['id'] == created['id'] for x in call('GET', base, 200))
        call('PUT', f"{base}/{created['id']}", 204, {'nome': name + ' atualizado'})
        assert call('GET', f"{base}/{created['id']}", 200)['nome'] == name + ' atualizado'
        call('POST', base, 400, {'nome': '  '})
        call('GET', base + '/999999', 404)
        call('PUT', base + '/999999', 404, {'nome': name})
        call('DELETE', base + '/999999', 404)
    author2 = call('POST', '/api/v1/autores', 201, {'nome': 'Coautor de teste'})['id']
    genre2 = call('POST', '/api/v1/generos', 201, {'nome': 'Clássico'})['id']
    user = call('POST', '/api/v1/usuarios', 201, {'nome': 'Guilherme', 'email': 'guilherme@example.com'})
    other = call('POST', '/api/v1/usuarios', 201, {'nome': 'Pedro', 'email': 'pedro@example.com'})
    uid, oid = user['id'], other['id']
    assert len(call('GET', '/api/v1/usuarios', 200)) == 2
    assert call('GET', f'/api/v1/usuarios/{uid}', 200)['nome'] == 'Guilherme'
    call('PUT', f'/api/v1/usuarios/{uid}', 204, {'nome': 'Guilherme atualizado', 'email': 'GUILHERME@example.com'})
    assert call('GET', f'/api/v1/usuarios/{uid}', 200)['email'] == 'guilherme@example.com'
    call('POST', '/api/v1/usuarios', 400, {'nome': 'Duplicado', 'email': 'GUILHERME@example.com'})
    call('PUT', f'/api/v1/usuarios/{uid}', 400, {'nome': 'Outro', 'email': 'pedro@example.com'})
    call('POST', '/api/v1/usuarios', 400, {'nome': 'Sem email', 'email': 'invalido'})
    call('GET', '/api/v1/usuarios/999999', 404)
    call('PUT', '/api/v1/usuarios/999999', 404, {'nome': 'Ausente', 'email': 'ausente@example.com'})
    call('DELETE', '/api/v1/usuarios/999999', 404)
    book = {'titulo': 'Dom Casmurro', 'isbn': '978-85-359-1484-9', 'editora': 'Companhia das Letras',
            'dataPublicacao': '2016-01-01', 'numeroPaginas': 256,
            'capaUrl': 'https://covers.openlibrary.org/b/isbn/9788535914849-L.jpg',
            'autorIds': [refs['autores'], author2], 'generoIds': [refs['generos'], genre2]}
    saved = call('POST', '/api/v1/livros', 201, book)
    bid = saved['id']
    assert saved['isbn'] == '9788535914849'
    assert len(saved['autores']) == len(saved['generos']) == 2
    assert saved['capaUrl'] == book['capaUrl']
    assert call('GET', f'/api/v1/livros/{bid}', 200)['titulo'] == book['titulo']
    assert len(call('GET', '/api/v1/livros', 200)) == 1
    call('POST', '/api/v1/livros', 400, {**book, 'isbn': '9788535914849'})
    for changes in [{'isbn': 'invalido'}, {'numeroPaginas': 0}, {'dataPublicacao': None},
                    {'capaUrl': 'file:///tmp/capa.png'}, {'autorIds': []}, {'generoIds': [999999]},
                    {'autorIds': [author2, author2]}, {'autorIds': [-1]}, {'titulo': '  '}]:
        call('POST', '/api/v1/livros', 400, {**book, 'isbn': '9788535914848', **changes})
    changed = {**book, 'titulo': 'Dom Casmurro atualizado', 'autorIds': [refs['autores']], 'generoIds': [refs['generos']]}
    call('PUT', f'/api/v1/livros/{bid}', 204, changed)
    result = call('GET', f'/api/v1/livros/{bid}', 200)
    assert result['titulo'] == changed['titulo'] and len(result['autores']) == len(result['generos']) == 1
    spare = call('POST', '/api/v1/livros', 201, {**book, 'isbn': '9788535914848'})['id']
    call('PUT', f'/api/v1/livros/{spare}', 400, changed)
    call('GET', '/api/v1/livros/999999', 404)
    call('PUT', '/api/v1/livros/999999', 404, book)
    call('DELETE', '/api/v1/livros/999999', 404)
    path = f'/api/v1/usuarios/{uid}/livros/{bid}'
    otherpath = f'/api/v1/usuarios/{oid}/livros/{bid}'
    assert call('GET', f'/api/v1/usuarios/{uid}/livros', 200) == []
    call('PUT', path, 201, {'status': 'QueroLer', 'paginaAtual': 0})
    call('PUT', otherpath, 201, {'status': 'Abandonado', 'paginaAtual': 10, 'nota': 0})
    reading = {'status': 'Lendo', 'paginaAtual': 80, 'nota': 4, 'avaliacao': 'Narrador interessante.'}
    call('PUT', path, 204, reading)
    call('PUT', path, 204, reading)
    assert call('GET', path, 200)['nota'] == 4
    assert call('GET', otherpath, 200)['status'] == 'Abandonado'
    assert len(call('GET', f'/api/v1/usuarios/{uid}/livros', 200)) == 1
    for changes in [{'nota': 6}, {'nota': -1}, {'nota': 3.5}, {'paginaAtual': 257}, {'paginaAtual': -1},
                    {'status': 'Invalido'}, {'status': 99}, {'status': 1}, {'status': None}, {'avaliacao': 'x' * 2001}]:
        call('PUT', path, 400, {**reading, **changes})
    for status in ['Lido', 'Relendo', 'QueroLer', 'Abandonado', 'Lendo']:
        call('PUT', path, 204, {**reading, 'status': status})
        assert call('GET', path, 200)['status'] == status
    call('GET', '/api/v1/usuarios/999999/livros', 404)
    call('PUT', f'/api/v1/usuarios/999999/livros/{bid}', 404, reading)
    call('PUT', f'/api/v1/usuarios/{uid}/livros/999999', 404, reading)
    call('GET', f'/api/v1/usuarios/{uid}/livros/999999', 404)
    call('DELETE', f'/api/v1/usuarios/{uid}/livros/999999', 404)
    call('PUT', f'/api/v1/livros/{bid}', 400, {**changed, 'numeroPaginas': 50})
    call('DELETE', f'/api/v1/livros/{bid}', 400)
    call('DELETE', f"/api/v1/autores/{refs['autores']}", 400)
    call('DELETE', f"/api/v1/generos/{refs['generos']}", 400)
    call('GET', '/api/v2/livros', 404)
    call('DELETE', path, 204)
    call('GET', path, 404)
    assert call('GET', otherpath, 200)['status'] == 'Abandonado'
    call('DELETE', f'/api/v1/usuarios/{oid}', 204)
    call('GET', otherpath, 404)
    call('DELETE', f'/api/v1/livros/{bid}', 204)
    call('DELETE', f'/api/v1/livros/{spare}', 204)
    call('GET', f'/api/v1/livros/{bid}', 404)
    for resource, ids in [('autores', [refs['autores'], author2]), ('generos', [refs['generos'], genre2])]:
        for id_ in ids:
            call('DELETE', f'/api/v1/{resource}/{id_}', 204)
            call('GET', f'/api/v1/{resource}/{id_}', 404)
    assert call('GET', '/api/v1/livros', 200) == []
    assert call('GET', f'/api/v1/usuarios/{uid}/livros', 200) == []
    return uid


def main():
    subprocess.run(['dotnet', 'build', '--nologo'], cwd=ROOT, check=True)
    with ApiProcess() as server:
        client = HttpClient(server.url)
        uid = verify(client)
        document = json.load(urllib.request.urlopen(server.url + '/swagger/v1/swagger.json'))
        advertised = {(m.upper(), p) for p, item in document['paths'].items() for m in item
                      if m in {'get', 'post', 'put', 'delete'}}
        assert advertised <= client.operations, f'Endpoints não verificados: {advertised - client.operations}'
        server.restart()
        assert client.request('GET', f'/api/v1/usuarios/{uid}', 200)['nome'] == 'Guilherme atualizado'
        with sqlite3.connect(server.database) as db:
            assert db.execute('SELECT COUNT(*) FROM __EFMigrationsHistory').fetchone()[0] == 1
            assert db.execute('PRAGMA foreign_key_check').fetchall() == []
        print(f'Verificação concluída: {client.count} requisições; {len(advertised)} operações; persistência após reinício; migration aplicada.')


if __name__ == '__main__':
    main()
