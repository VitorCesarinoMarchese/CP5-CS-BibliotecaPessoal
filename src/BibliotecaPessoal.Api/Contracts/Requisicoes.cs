using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BibliotecaPessoal.Api.Models;

namespace BibliotecaPessoal.Api.Contracts;

public sealed class NomeRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Nome { get; init; } = "";
}

public sealed class UsuarioRequest
{
    [Required, StringLength(150)]
    public string Nome { get; init; } = "";
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = "";
}

public sealed class LivroRequest : IValidatableObject
{
    [Required, StringLength(250)]
    public string Titulo { get; init; } = "";
    [Required, StringLength(32)]
    public string Isbn { get; init; } = "";
    [Required, StringLength(150)]
    public string Editora { get; init; } = "";
    [Required]
    public DateOnly? DataPublicacao { get; init; }
    [Range(1, int.MaxValue)]
    public int NumeroPaginas { get; init; }
    [StringLength(2048)]
    public string? CapaUrl { get; init; }
    [Required, MinLength(1)]
    public int[] AutorIds { get; init; } = [];
    [Required, MinLength(1)]
    public int[] GeneroIds { get; init; } = [];

    [System.Text.Json.Serialization.JsonIgnore]
    public string IsbnNormalizado => Isbn.Replace("-", "").Replace(" ", "").ToUpperInvariant();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!Regex.IsMatch(IsbnNormalizado, @"^(\d{13}|\d{9}[\dX])$"))
            yield return new("Informe ISBN com 10 ou 13 caracteres, permitindo espaços e hífens.", [nameof(Isbn)]);
        if (CapaUrl is not null && (!Uri.TryCreate(CapaUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https")))
            yield return new("A capa deve ser uma URL HTTP ou HTTPS absoluta.", [nameof(CapaUrl)]);
        if (AutorIds.Any(x => x <= 0) || AutorIds.Distinct().Count() != AutorIds.Length)
            yield return new("Informe IDs de autores positivos, sem repetições.", [nameof(AutorIds)]);
        if (GeneroIds.Any(x => x <= 0) || GeneroIds.Distinct().Count() != GeneroIds.Length)
            yield return new("Informe IDs de gêneros positivos, sem repetições.", [nameof(GeneroIds)]);
    }
}

public sealed class LeituraRequest
{
    [Required, EnumDataType(typeof(StatusLeitura))]
    public StatusLeitura? Status { get; init; }
    [Range(0, int.MaxValue)]
    public int PaginaAtual { get; init; }
    [Range(0, 5)]
    public int? Nota { get; init; }
    [StringLength(2000)]
    public string? Avaliacao { get; init; }
}
