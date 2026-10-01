using BibliotecaPessoal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Data;

public sealed class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Genero> Generos => Set<Genero>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Leitura> Leituras => Set<Leitura>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Autor>().Property(x => x.Nome).HasMaxLength(150).IsRequired();
        model.Entity<Genero>().Property(x => x.Nome).HasMaxLength(100).IsRequired();
        model.Entity<Usuario>(entity =>
        {
            entity.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });
        model.Entity<Livro>(entity =>
        {
            entity.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Isbn).HasMaxLength(13).IsRequired();
            entity.HasIndex(x => x.Isbn).IsUnique();
            entity.Property(x => x.Editora).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CapaUrl).HasMaxLength(2048);
            entity.HasMany(x => x.Autores).WithMany(x => x.Livros).UsingEntity("LivroAutores");
            entity.HasMany(x => x.Generos).WithMany(x => x.Livros).UsingEntity("LivroGeneros");
            entity.ToTable(table => table.HasCheckConstraint("CK_Livro_Paginas", "NumeroPaginas > 0"));
        });
        model.Entity<Leitura>(entity =>
        {
            entity.HasKey(x => new { x.UsuarioId, x.LivroId });
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Avaliacao).HasMaxLength(2000);
            entity.HasOne(x => x.Usuario).WithMany(x => x.Leituras).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Livro).WithMany(x => x.Leituras).HasForeignKey(x => x.LivroId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Leitura_Nota", "Nota IS NULL OR Nota BETWEEN 0 AND 5");
                table.HasCheckConstraint("CK_Leitura_Pagina", "PaginaAtual >= 0");
                table.HasCheckConstraint("CK_Leitura_Status", "Status IN ('QueroLer', 'Lendo', 'Lido', 'Abandonado', 'Relendo')");
            });
        });
    }
}
