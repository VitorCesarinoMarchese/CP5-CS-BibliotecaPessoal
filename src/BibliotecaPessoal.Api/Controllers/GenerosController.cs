using BibliotecaPessoal.Api.Contracts;
using BibliotecaPessoal.Api.Data;
using BibliotecaPessoal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Controllers;

[ApiController]
[Route("api/v1/generos")]
public sealed class GenerosController(BibliotecaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NomeResponse>>> Listar(CancellationToken ct) =>
        await db.Generos.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new NomeResponse(x.Id, x.Nome)).ToListAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NomeResponse>> Obter(int id, CancellationToken ct)
    {
        var item = await db.Generos.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new NomeResponse(x.Id, x.Nome)).SingleOrDefaultAsync(ct);
        return item is null ? Problem(statusCode: 404, detail: "Registro não encontrado.") : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<NomeResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<NomeResponse>> Criar(NomeRequest request, CancellationToken ct)
    {
        var item = new Genero { Nome = request.Nome.Trim() };
        db.Generos.Add(item);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = item.Id }, new NomeResponse(item.Id, item.Nome));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(int id, NomeRequest request, CancellationToken ct)
    {
        var item = await db.Generos.FindAsync([id], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Registro não encontrado.");
        item.Nome = request.Nome.Trim();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var item = await db.Generos.FindAsync([id], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Registro não encontrado.");
        if (await db.Livros.AnyAsync(x => x.Generos.Any(a => a.Id == id), ct))
            return Problem(statusCode: 400, detail: "Remova a associação aos livros antes de excluir este registro.");
        db.Generos.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
