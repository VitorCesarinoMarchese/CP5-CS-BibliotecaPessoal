# Evidências do Swagger

Capturas reais de requisições executadas pelo botão Execute do Swagger UI, contra a API .NET 10 e uma base SQLite temporária com a migration aplicada.

Cada imagem contém a operação, os parâmetros e a resposta HTTP. O teste confere o status e a cobertura de todas as operações descritas no OpenAPI.

| Método | Rota | Status | Print |
|---|---|---|---|
| POST | `/api/v1/autores` | 201 | [Abrir](01-post-api-v1-autores-201.png) |
| GET | `/api/v1/autores/{id}` | 200 | [Abrir](02-get-api-v1-autores-id-200.png) |
| GET | `/api/v1/autores` | 200 | [Abrir](03-get-api-v1-autores-200.png) |
| PUT | `/api/v1/autores/{id}` | 204 | [Abrir](04-put-api-v1-autores-id-204.png) |
| POST | `/api/v1/generos` | 201 | [Abrir](05-post-api-v1-generos-201.png) |
| GET | `/api/v1/generos/{id}` | 200 | [Abrir](06-get-api-v1-generos-id-200.png) |
| GET | `/api/v1/generos` | 200 | [Abrir](07-get-api-v1-generos-200.png) |
| PUT | `/api/v1/generos/{id}` | 204 | [Abrir](08-put-api-v1-generos-id-204.png) |
| POST | `/api/v1/usuarios` | 201 | [Abrir](09-post-api-v1-usuarios-201.png) |
| GET | `/api/v1/usuarios` | 200 | [Abrir](10-get-api-v1-usuarios-200.png) |
| GET | `/api/v1/usuarios/{id}` | 200 | [Abrir](11-get-api-v1-usuarios-id-200.png) |
| PUT | `/api/v1/usuarios/{id}` | 204 | [Abrir](12-put-api-v1-usuarios-id-204.png) |
| POST | `/api/v1/livros` | 201 | [Abrir](13-post-api-v1-livros-201.png) |
| GET | `/api/v1/livros/{id}` | 200 | [Abrir](14-get-api-v1-livros-id-200.png) |
| GET | `/api/v1/livros` | 200 | [Abrir](15-get-api-v1-livros-200.png) |
| POST | `/api/v1/livros` | 400 | [Abrir](16-post-api-v1-livros-400.png) |
| PUT | `/api/v1/livros/{id}` | 204 | [Abrir](17-put-api-v1-livros-id-204.png) |
| GET | `/api/v1/livros/{id}` | 404 | [Abrir](18-get-api-v1-livros-id-404.png) |
| GET | `/api/v1/usuarios/{usuarioId}/livros` | 200 | [Abrir](19-get-api-v1-usuarios-usuarioid-livros-200.png) |
| PUT | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | 201 | [Abrir](20-put-api-v1-usuarios-usuarioid-livros-livroid-201.png) |
| PUT | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | 204 | [Abrir](21-put-api-v1-usuarios-usuarioid-livros-livroid-204.png) |
| GET | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | 200 | [Abrir](22-get-api-v1-usuarios-usuarioid-livros-livroid-200.png) |
| PUT | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | 400 | [Abrir](23-put-api-v1-usuarios-usuarioid-livros-livroid-400.png) |
| DELETE | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | 204 | [Abrir](24-delete-api-v1-usuarios-usuarioid-livros-livroid-204.png) |
| DELETE | `/api/v1/usuarios/{id}` | 204 | [Abrir](25-delete-api-v1-usuarios-id-204.png) |
| DELETE | `/api/v1/livros/{id}` | 204 | [Abrir](26-delete-api-v1-livros-id-204.png) |
| DELETE | `/api/v1/autores/{id}` | 204 | [Abrir](27-delete-api-v1-autores-id-204.png) |
| DELETE | `/api/v1/generos/{id}` | 204 | [Abrir](28-delete-api-v1-generos-id-204.png) |

## Reproduzir

Instale Playwright em um ambiente Python e execute na raiz do repositório:

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r scripts/requirements-evidence.txt
python scripts/capture-swagger.py
```

O script usa Chromium instalado em `/usr/bin/chromium`. Defina `CHROMIUM_PATH` para usar outro caminho. Ele cria e apaga seu próprio banco temporário.
