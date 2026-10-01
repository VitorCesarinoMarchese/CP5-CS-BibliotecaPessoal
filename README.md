# Biblioteca Pessoal

API RESTful para cadastrar livros e organizar a leitura de várias pessoas. O catálogo reúne título, autores, gêneros, ISBN, editora, data de publicação, número de páginas e URL da capa. Cada usuário mantém seu status de leitura, página atual, nota e avaliação.

O projeto atende leitores que querem organizar o que pretendem ler e acompanhar o que já leram. Foi desenvolvido para o CP5 da disciplina C# Software Development, com C# .NET 10 e Entity Framework Core 10.

## Integrantes

| Nome | RM |
|---|---|
| Guilherme | 554962 |
| Pedro | 555556 |
| Fabrício | 558216 |
| Vitor | 554893 |
| Matheus | 555447 |

## Banco de dados

SQLite, acessado pelo Entity Framework Core. O arquivo `biblioteca.db` é criado no diretório do projeto da API pelo comando de migration. O banco e os arquivos de compilação ficam fora do Git.

O modelo contém usuários, autores, gêneros, livros e leituras. Livros têm relacionamentos muitos para muitos com autores e gêneros. Leituras usam a chave composta `UsuarioId` e `LivroId`.

## Executar localmente

Instale o SDK .NET 10. Execute na raiz do repositório:

```bash
dotnet tool restore
dotnet restore
dotnet ef database update --project src/BibliotecaPessoal.Api
dotnet run --project src/BibliotecaPessoal.Api --launch-profile http
```

A API atende em `http://localhost:5080`. Abra [Swagger UI](http://localhost:5080/swagger/index.html) para consultar contratos e executar requisições. O Swagger está habilitado no ambiente Development, definido pelo perfil `http`.

Para trocar o banco ou o local do arquivo, altere `ConnectionStrings:Biblioteca` em `appsettings.json`. Também é possível usar a variável `ConnectionStrings__Biblioteca`. Use a mesma configuração na migration e na execução.

## Migration

A migration `InitialCreate`, em `src/BibliotecaPessoal.Api/Migrations`, cria as tabelas, os relacionamentos, os índices únicos de ISBN e e-mail e as constraints de nota, status e páginas.

O esquema foi gerado com:

```bash
dotnet ef migrations add InitialCreate --project src/BibliotecaPessoal.Api
```

Para listar e aplicar as migrations existentes:

```bash
dotnet ef migrations list --project src/BibliotecaPessoal.Api
dotnet ef database update --project src/BibliotecaPessoal.Api
```

Não use `EnsureCreated` junto das migrations. Para uma alteração futura no modelo, gere uma nova migration e aplique com `database update`.

## Endpoints disponíveis

Todas as rotas de negócio usam a versão `/api/v1`. Rotas de versões não implementadas, como `/api/v2/livros`, retornam 404.

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| GET | `/api/v1/autores` | Listar autores | 200 |
| GET | `/api/v1/autores/{id}` | Consultar autor | 200 |
| POST | `/api/v1/autores` | Cadastrar autor | 201 |
| PUT | `/api/v1/autores/{id}` | Substituir nome do autor | 204 |
| DELETE | `/api/v1/autores/{id}` | Excluir autor sem livros associados | 204 |
| GET | `/api/v1/generos` | Listar gêneros | 200 |
| GET | `/api/v1/generos/{id}` | Consultar gênero | 200 |
| POST | `/api/v1/generos` | Cadastrar gênero | 201 |
| PUT | `/api/v1/generos/{id}` | Substituir nome do gênero | 204 |
| DELETE | `/api/v1/generos/{id}` | Excluir gênero sem livros associados | 204 |
| GET | `/api/v1/usuarios` | Listar usuários | 200 |
| GET | `/api/v1/usuarios/{id}` | Consultar usuário | 200 |
| POST | `/api/v1/usuarios` | Cadastrar usuário | 201 |
| PUT | `/api/v1/usuarios/{id}` | Substituir nome e e-mail | 204 |
| DELETE | `/api/v1/usuarios/{id}` | Excluir usuário e seus acompanhamentos | 204 |
| GET | `/api/v1/livros` | Listar catálogo com autores e gêneros | 200 |
| GET | `/api/v1/livros/{id}` | Consultar livro | 200 |
| POST | `/api/v1/livros` | Cadastrar livro | 201 |
| PUT | `/api/v1/livros/{id}` | Substituir dados e associações do livro | 204 |
| DELETE | `/api/v1/livros/{id}` | Excluir livro fora das bibliotecas dos usuários | 204 |
| GET | `/api/v1/usuarios/{usuarioId}/livros` | Listar biblioteca e acompanhamentos do usuário | 200 |
| GET | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | Consultar acompanhamento | 200 |
| PUT | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | Incluir ou substituir acompanhamento e avaliação | 201 ou 204 |
| DELETE | `/api/v1/usuarios/{usuarioId}/livros/{livroId}` | Remover da biblioteca pessoal, mantendo o catálogo | 204 |

POST retorna `201 Created` com o recurso e o header `Location`. PUT de acompanhamento usa uma URI definida pelo usuário e livro: a primeira inclusão retorna 201 com `Location`; repetir ou substituir retorna 204 sem criar duplicatas. Atualizações e exclusões com 204 têm corpo vazio.

Entradas inválidas, ISBN ou e-mail duplicados e exclusões bloqueadas retornam 400. Registros inexistentes retornam 404. Erros usam `ProblemDetails`; falhas inesperadas retornam 500 sem detalhes internos do banco.

## Experimentar no Swagger

Crie primeiro um autor, um gênero e um usuário. Copie os IDs retornados para cadastrar o livro e o acompanhamento.

Autor ou gênero:

```json
{ "nome": "Machado de Assis" }
```

Usuário:

```json
{ "nome": "Guilherme", "email": "guilherme@example.com" }
```

Livro:

```json
{
  "titulo": "Dom Casmurro",
  "isbn": "9788535914849",
  "editora": "Companhia das Letras",
  "dataPublicacao": "2016-01-01",
  "numeroPaginas": 256,
  "capaUrl": "https://covers.openlibrary.org/b/isbn/9788535914849-L.jpg",
  "autorIds": [1],
  "generoIds": [1]
}
```

Acompanhamento em `PUT /api/v1/usuarios/1/livros/1`:

```json
{
  "status": "Lendo",
  "paginaAtual": 80,
  "nota": 4,
  "avaliacao": "Narrador interessante."
}
```

Os exemplos de livro servem como dados de demonstração. Use os IDs efetivamente retornados pela sua base.

## Regras e escopo

- Os estados são `QueroLer`, `Lendo`, `Lido`, `Abandonado` e `Relendo`. O JSON usa os nomes em texto, sem valores numéricos.
- Status é obrigatório no acompanhamento. Cada pessoa pode mudar livremente entre os estados.
- Página atual deve estar entre zero e o total de páginas. O status não altera a página automaticamente.
- Nota é opcional e aceita inteiros de 0 a 5. Avaliação é opcional e aceita até 2.000 caracteres.
- ISBN é obrigatório, com 10 ou 13 caracteres. Espaços e hífens são removidos e `x` vira `X` antes de conferir unicidade. A API valida o formato, sem calcular o dígito verificador.
- Livro exige título, editora, data de publicação, páginas positivas e pelo menos um autor e um gênero existentes. IDs repetidos são rejeitados.
- `autorIds` e `generoIds` substituem todas as associações no PUT do livro.
- A capa é opcional. `capaUrl` armazena uma URL HTTP ou HTTPS; a API não recebe arquivos nem baixa imagens. A disponibilidade da imagem depende do servidor externo.
- Usuários têm nome e e-mail único, normalizado em minúsculas.
- Excluir usuário remove seus acompanhamentos. Excluir livro exige removê-lo antes das bibliotecas pessoais. Autores e gêneros associados a livros também têm exclusão bloqueada.
- Reduzir o número de páginas de um livro abaixo do progresso de algum usuário retorna 400.
- O projeto permite vários cadastros de usuários, sem login ou controle de acesso. Os endpoints estão abertos; qualquer cliente pode consultar e alterar os dados.

## Verificar a API

Com Python 3 e o SDK .NET 10 instalados, execute:

```bash
dotnet tool restore
python scripts/verify-api.py
```

O workflow `.github/workflows/verify.yml` executa esta verificação em pushes e pull requests.

O script compila, aplica a migration em SQLite temporário, inicia a API e verifica todas as 24 operações do Swagger. Também confere duplicatas, entradas inválidas, relações muitos para muitos, separação entre usuários, mudanças de estado, exclusões e persistência após reiniciar. Ele não altera seu banco local.

## Evidências de teste

Os [prints reais do Swagger](docs/evidencias/README.md) demonstram cada endpoint funcionando, incluindo respostas 200, 201, 204, 400 e 404. O [manifesto](docs/evidencias/manifest.json) relaciona rota, método, status e arquivo de cada captura.

O procedimento de reprodução está junto às evidências. A captura executa requisições pelo botão Execute do Swagger e confere a cobertura com o documento OpenAPI.

## Organização

`Directory.Build.props` permite compilar com distribuições do SDK que não incluem os metadados opcionais de remoção de dependências transitivas, como a instalação Arch Linux usada nesta verificação.

- `src/BibliotecaPessoal.Api/Controllers`: rotas e operações HTTP.
- `src/BibliotecaPessoal.Api/Contracts`: DTOs, validações e respostas.
- `src/BibliotecaPessoal.Api/Models`: entidades e estados de leitura.
- `src/BibliotecaPessoal.Api/Data`: DbContext e mapeamentos EF Core.
- `src/BibliotecaPessoal.Api/Infrastructure`: tratamento de exceções.
- `src/BibliotecaPessoal.Api/Migrations`: histórico do banco.
- `scripts`: verificação HTTP e captura de evidências.
- `docs`: [decisões de arquitetura](docs/arquitetura.md) e prints.

As migrations seguem a [documentação oficial do EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing). A interface de testes usa [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).
