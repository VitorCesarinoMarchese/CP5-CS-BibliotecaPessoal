using BibliotecaPessoal.Api.Contracts;
using BibliotecaPessoal.Api.Data;
using BibliotecaPessoal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
public sealed class UsuariosController(BibliotecaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> Listar(CancellationToken ct) =>
        await db.Usuarios.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new UsuarioResponse(x.Id, x.Nome, x.Email)).ToListAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> Obter(int id, CancellationToken ct)
    {
        var item = await db.Usuarios.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new UsuarioResponse(x.Id, x.Nome, x.Email)).SingleOrDefaultAsync(ct);
        return item is null ? Problem(statusCode: 404, detail: "Usuário não encontrado.") : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioResponse>> Criar(UsuarioRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Email == email, ct))
            return Problem(statusCode: 400, detail: "Este e-mail já está cadastrado.");
        var item = new Usuario { Nome = request.Nome.Trim(), Email = email };
        db.Usuarios.Add(item);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = item.Id }, new UsuarioResponse(item.Id, item.Nome, item.Email));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(int id, UsuarioRequest request, CancellationToken ct)
    {
        var item = await db.Usuarios.FindAsync([id], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Usuário não encontrado.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Id != id && x.Email == email, ct))
            return Problem(statusCode: 400, detail: "Este e-mail já está cadastrado.");
        item.Nome = request.Nome.Trim();
        item.Email = email;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var item = await db.Usuarios.FindAsync([id], ct);
        if (item is null) return Problem(statusCode: 404, detail: "Usuário não encontrado.");
        db.Usuarios.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
