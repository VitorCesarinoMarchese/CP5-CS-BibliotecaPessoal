# Arquitetura

Um usuário cadastra autores e gêneros, adiciona um livro ao catálogo e salva seu acompanhamento em `PUT /api/v1/usuarios/{usuarioId}/livros/{livroId}`. A primeira inclusão responde 201; as substituições respondem 204. Outro usuário acompanha o mesmo livro com status e avaliação próprios.

## Estrutura escolhida

Controllers usam o `BibliotecaDbContext` diretamente. `Contracts` define entradas validadas e projeções de respostas. `Models` contém as entidades. `Data` centraliza relacionamentos, índices e constraints. `Infrastructure` trata falhas como ProblemDetails. `Migrations` registra o esquema SQLite.

`Livro` tem muitos autores e gêneros. `Leitura` tem chave composta por usuário e livro e armazena status, página, nota e avaliação. ISBN normalizado e e-mail normalizado têm índices únicos. Nota e página também têm constraints no banco.

## Comparação dos desenhos

Foram comparados dois desenhos: Controllers com EF direto e dois serviços de aplicação para catálogo e leitura. A revisão independente pontuou o primeiro em 24/25 e o segundo em 22/25, considerando escopo, simplicidade, invariantes, verificação e viabilidade para o prazo.

Escolhemos o primeiro por manter as operações visíveis sem métodos de repasse. Incorporamos do segundo as constraints, o tratamento de conflitos SQLite e a restrição de exclusão de livros presentes nas bibliotecas. Um usuário excluído remove seus acompanhamentos; o catálogo permanece.

Não criamos repository sobre o EF Core nem um serviço por recurso. As rotas incluem explicitamente `/api/v1`; versões não implementadas não têm endpoints. A capa é uma URL externa, sem download feito pelo servidor. Usuários são cadastros sem login ou autorização.

## Verificação

`scripts/verify-api.py` executa o ciclo HTTP completo em um banco temporário, confere isolamento de estados entre usuários, valida entradas inválidas e confirma persistência após reiniciar o processo. A captura do Swagger reaplica esses casos pela interface e salva screenshots reais.
