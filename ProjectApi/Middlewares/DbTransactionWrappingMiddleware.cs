using ProjectApi.Data.Contexts;

namespace ProjectApi.Middlewares;

public class DbTransactionWrappingMiddleware
{
    private readonly RequestDelegate _next;

    public DbTransactionWrappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext applicationDbContext)
    {
        var transaction = await applicationDbContext.Database.BeginTransactionAsync();
        await _next(httpContext);
        await transaction.CommitAsync();
    }
}