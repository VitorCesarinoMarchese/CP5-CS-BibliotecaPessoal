using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaPessoal.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Isbn = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Editora = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    DataPublicacao = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    NumeroPaginas = table.Column<int>(type: "INTEGER", nullable: false),
                    CapaUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Id);
                    table.CheckConstraint("CK_Livro_Paginas", "NumeroPaginas > 0");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LivroAutores",
                columns: table => new
                {
                    AutoresId = table.Column<int>(type: "INTEGER", nullable: false),
                    LivrosId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivroAutores", x => new { x.AutoresId, x.LivrosId });
                    table.ForeignKey(
                        name: "FK_LivroAutores_Autores_AutoresId",
                        column: x => x.AutoresId,
                        principalTable: "Autores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivroAutores_Livros_LivrosId",
                        column: x => x.LivrosId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivroGeneros",
                columns: table => new
                {
                    GenerosId = table.Column<int>(type: "INTEGER", nullable: false),
                    LivrosId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivroGeneros", x => new { x.GenerosId, x.LivrosId });
                    table.ForeignKey(
                        name: "FK_LivroGeneros_Generos_GenerosId",
                        column: x => x.GenerosId,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivroGeneros_Livros_LivrosId",
                        column: x => x.LivrosId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leituras",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    LivroId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PaginaAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    Nota = table.Column<int>(type: "INTEGER", nullable: true),
                    Avaliacao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leituras", x => new { x.UsuarioId, x.LivroId });
                    table.CheckConstraint("CK_Leitura_Nota", "Nota IS NULL OR Nota BETWEEN 0 AND 5");
                    table.CheckConstraint("CK_Leitura_Pagina", "PaginaAtual >= 0");
                    table.CheckConstraint("CK_Leitura_Status", "Status IN ('QueroLer', 'Lendo', 'Lido', 'Abandonado', 'Relendo')");
                    table.ForeignKey(
                        name: "FK_Leituras_Livros_LivroId",
                        column: x => x.LivroId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leituras_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_LivroId",
                table: "Leituras",
                column: "LivroId");

            migrationBuilder.CreateIndex(
                name: "IX_LivroAutores_LivrosId",
                table: "LivroAutores",
                column: "LivrosId");

            migrationBuilder.CreateIndex(
                name: "IX_LivroGeneros_LivrosId",
                table: "LivroGeneros",
                column: "LivrosId");

            migrationBuilder.CreateIndex(
                name: "IX_Livros_Isbn",
                table: "Livros",
                column: "Isbn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leituras");

            migrationBuilder.DropTable(
                name: "LivroAutores");

            migrationBuilder.DropTable(
                name: "LivroGeneros");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Autores");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropTable(
                name: "Livros");
        }
    }
}
