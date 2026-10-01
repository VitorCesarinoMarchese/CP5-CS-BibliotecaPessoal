namespace BibliotecaPessoal.Api.Models;

public enum StatusLeitura { QueroLer, Lendo, Lido, Abandonado, Relendo }

public sealed class Autor
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public List<Livro> Livros { get; set; } = [];
}

public sealed class Genero
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public List<Livro> Livros { get; set; } = [];
}

public sealed class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public List<Leitura> Leituras { get; set; } = [];
}

public sealed class Livro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Isbn { get; set; } = "";
    public string Editora { get; set; } = "";
    public DateOnly DataPublicacao { get; set; }
    public int NumeroPaginas { get; set; }
    public string? CapaUrl { get; set; }
    public List<Autor> Autores { get; set; } = [];
    public List<Genero> Generos { get; set; } = [];
    public List<Leitura> Leituras { get; set; } = [];
}

public sealed class Leitura
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int LivroId { get; set; }
    public Livro Livro { get; set; } = null!;
    public StatusLeitura Status { get; set; }
    public int PaginaAtual { get; set; }
    public int? Nota { get; set; }
    public string? Avaliacao { get; set; }
}
