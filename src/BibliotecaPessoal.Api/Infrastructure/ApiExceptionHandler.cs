using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPessoal.Api.Infrastructure;

public sealed class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var constraint = exception is DbUpdateException { InnerException: SqliteException { SqliteErrorCode: 19 } };
        var status = constraint ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        if (!constraint) logger.LogError(exception, "Falha ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = constraint ? "Dados em conflito" : "Erro interno",
                Detail = constraint
                    ? "A operação viola uma regra do banco: registro duplicado, associação inválida ou valor fora do limite."
                    : "Não foi possível concluir a operação."
            }
        });
    }
}
