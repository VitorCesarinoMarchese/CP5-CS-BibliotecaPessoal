using BibliotecaPessoal.Api.Contracts;
using BibliotecaPessoal.Api.Data;
using BibliotecaPessoal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Controllers;

[ApiController]
[Route("api/v1/livros")]
public sealed class LivrosController(BibliotecaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LivroResponse>>> Listar(CancellationToken ct) =>
        await db.Livros.AsNoTracking().OrderBy(x => x.Id).Select(Projecoes.Livro).ToListAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroResponse>> Obter(int id, CancellationToken ct)
    {
        var item = await db.Livros.AsNoTracking().Where(x => x.Id == id).Select(Projecoes.Livro).SingleOrDefaultAsync(ct);
        return item is null ? Problem(statusCode: 404, detail: "Livro não encontrado.") : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<LivroResponse>> Criar(LivroRequest request, CancellationToken ct)
    {
        if (await db.Livros.AnyAsync(x => x.Isbn == request.IsbnNormalizado, ct))
            return Problem(statusCode: 400, detail: "Este ISBN já está cadastrado.");
        var autores = await db.Autores.Where(x => request.AutorIds.Contains(x.Id)).ToListAsync(ct);
        var generos = await db.Generos.Where(x => request.GeneroIds.Contains(x.Id)).ToListAsync(ct);
        if (autores.Count != request.AutorIds.Length || generos.Count != request.GeneroIds.Length)
            return Problem(statusCode: 400, detail: "Um ou mais autores ou gêneros não existem.");
        var item = new Livro();
        Aplicar(item, request, autores, generos);
        db.Livros.Add(item);
        await db.SaveChangesAsync(ct);
        var response = await db.Livros.AsNoTracking().Where(x => x.Id == item.Id).Select(Projecoes.Livro).SingleAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = item.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(int id, LivroRequest request, CancellationToken ct)
    {
        var item = await db.Livros.Include(x => x.Autores).Include(x => x.Generos).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return Problem(statusCode: 404, detail: "Livro não encontrado.");
        if (await db.Livros.AnyAsync(x => x.Id != id && x.Isbn == request.IsbnNormalizado, ct))
            return Problem(statusCode: 400, detail: "Este ISBN já está cadastrado.");
        if (await db.Leituras.AnyAsync(x => x.LivroId == id && x.PaginaAtual > request.NumeroPaginas, ct))
            return Problem(statusCode: 400, detail: "O número de páginas não pode ser menor que o progresso registrado.");
        var autores = await db.Autores.Where(x => request.AutorIds.Contains(x.Id)).ToListAsync(ct);
        var generos = await db.Generos.Where(x => request.GeneroIds.Contains(x.Id)).ToListAsync(ct);
        if (autores.Count != request.AutorIds.Length || generos.Count != request.GeneroIds.Length)
            return Problem(statusCode: 400, detail: "Um ou mais autores ou gêneros não existem.");
        Aplicar(item, request, autores, generos);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var item = await db.Livros.FindAsync([id], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Livro não encontrado.");
        if (await db.Leituras.AnyAsync(x => x.LivroId == id, ct))
            return Problem(statusCode: 400, detail: "Remova o livro das bibliotecas dos usuários antes de excluí-lo do catálogo.");
        db.Livros.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static void Aplicar(Livro item, LivroRequest request, List<Autor> autores, List<Genero> generos)
    {
        item.Titulo = request.Titulo.Trim();
        item.Isbn = request.IsbnNormalizado;
        item.Editora = request.Editora.Trim();
        item.DataPublicacao = request.DataPublicacao!.Value;
        item.NumeroPaginas = request.NumeroPaginas;
        item.CapaUrl = request.CapaUrl;
        item.Autores = autores;
        item.Generos = generos;
    }
}
