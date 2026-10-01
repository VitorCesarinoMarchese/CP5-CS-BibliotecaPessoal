using BibliotecaPessoal.Api.Contracts;
using BibliotecaPessoal.Api.Data;
using BibliotecaPessoal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Controllers;

[ApiController]
[Route("api/v1/usuarios/{usuarioId:int}/livros")]
public sealed class LeiturasController(BibliotecaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeituraResponse>>> Listar(int usuarioId, CancellationToken ct)
    {
        if (!await db.Usuarios.AnyAsync(x => x.Id == usuarioId, ct))
            return Problem(statusCode: 404, detail: "Usuário não encontrado.");
        return await db.Leituras.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .OrderBy(x => x.LivroId).Select(Projecoes.Leitura).ToListAsync(ct);
    }

    [HttpGet("{livroId:int}")]
    public async Task<ActionResult<LeituraResponse>> Obter(int usuarioId, int livroId, CancellationToken ct)
    {
        var item = await db.Leituras.AsNoTracking().Where(x => x.UsuarioId == usuarioId && x.LivroId == livroId)
            .Select(Projecoes.Leitura).SingleOrDefaultAsync(ct);
        return item is null ? Problem(statusCode: 404, detail: "Leitura não encontrada.") : Ok(item);
    }

    [HttpPut("{livroId:int}")]
    [ProducesResponseType<LeituraResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Salvar(int usuarioId, int livroId, LeituraRequest request, CancellationToken ct)
    {
        if (!await db.Usuarios.AnyAsync(x => x.Id == usuarioId, ct))
            return Problem(statusCode: 404, detail: "Usuário não encontrado.");
        var livro = await db.Livros.FindAsync([livroId], ct);
        if (livro is null) return Problem(statusCode: 404, detail: "Livro não encontrado.");
        if (request.PaginaAtual > livro.NumeroPaginas)
            return Problem(statusCode: 400, detail: "A página atual não pode ultrapassar o número de páginas do livro.");
        var item = await db.Leituras.FindAsync([usuarioId, livroId], ct);
        var nova = item is null;
        if (nova)
        {
            item = new Leitura { UsuarioId = usuarioId, LivroId = livroId };
            db.Leituras.Add(item);
        }
        item!.Status = request.Status!.Value;
        item.PaginaAtual = request.PaginaAtual;
        item.Nota = request.Nota;
        item.Avaliacao = request.Avaliacao?.Trim();
        await db.SaveChangesAsync(ct);
        if (!nova) return NoContent();
        var response = await db.Leituras.AsNoTracking().Where(x => x.UsuarioId == usuarioId && x.LivroId == livroId)
            .Select(Projecoes.Leitura).SingleAsync(ct);
        return CreatedAtAction(nameof(Obter), new { usuarioId, livroId }, response);
    }

    [HttpDelete("{livroId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(int usuarioId, int livroId, CancellationToken ct)
    {
        var item = await db.Leituras.FindAsync([usuarioId, livroId], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Leitura não encontrada.");
        db.Leituras.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
