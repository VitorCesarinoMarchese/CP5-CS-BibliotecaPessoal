using System.Linq.Expressions;
using BibliotecaPessoal.Api.Models;

namespace BibliotecaPessoal.Api.Contracts;

public sealed record NomeResponse(int Id, string Nome);
public sealed record UsuarioResponse(int Id, string Nome, string Email);
public sealed record LivroResponse(int Id, string Titulo, string Isbn, string Editora,
    DateOnly DataPublicacao, int NumeroPaginas, string? CapaUrl,
    List<NomeResponse> Autores, List<NomeResponse> Generos);
public sealed record LeituraResponse(int UsuarioId, StatusLeitura Status, int PaginaAtual,
    int? Nota, string? Avaliacao, LivroResponse Livro);

public static class Projecoes
{
    public static readonly Expression<Func<Livro, LivroResponse>> Livro = x => new(
        x.Id, x.Titulo, x.Isbn, x.Editora, x.DataPublicacao, x.NumeroPaginas, x.CapaUrl,
        x.Autores.OrderBy(a => a.Id).Select(a => new NomeResponse(a.Id, a.Nome)).ToList(),
        x.Generos.OrderBy(g => g.Id).Select(g => new NomeResponse(g.Id, g.Nome)).ToList());

    public static readonly Expression<Func<Leitura, LeituraResponse>> Leitura = x => new(
        x.UsuarioId, x.Status, x.PaginaAtual, x.Nota, x.Avaliacao,
        new LivroResponse(x.Livro.Id, x.Livro.Titulo, x.Livro.Isbn, x.Livro.Editora,
            x.Livro.DataPublicacao, x.Livro.NumeroPaginas, x.Livro.CapaUrl,
            x.Livro.Autores.OrderBy(a => a.Id).Select(a => new NomeResponse(a.Id, a.Nome)).ToList(),
            x.Livro.Generos.OrderBy(g => g.Id).Select(g => new NomeResponse(g.Id, g.Nome)).ToList()));
}
