using System.Text.Json.Serialization;
using BibliotecaPessoal.Api.Data;
using BibliotecaPessoal.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddDbContext<BibliotecaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Biblioteca")));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Biblioteca Pessoal API",
    Version = "v1",
    Description = "Catálogo compartilhado e acompanhamento de leitura por usuário. API acadêmica sem autenticação."
}));

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca Pessoal v1"));
}
app.MapControllers();
app.Run();
