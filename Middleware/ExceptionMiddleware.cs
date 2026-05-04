using System.Text.Json;

namespace velcro.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception non gérée sur {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            await HandleAsync(ctx, ex);
        }
    }

    private static async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            KeyNotFoundException e       => (StatusCodes.Status404NotFound,            e.Message),
            UnauthorizedAccessException e => (StatusCodes.Status403Forbidden,           e.Message),
            ArgumentException e          => (StatusCodes.Status400BadRequest,           e.Message),
            _                            => (StatusCodes.Status500InternalServerError,  "Une erreur interne s'est produite.")
        };

        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode  = status;
            ctx.Response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new { error = message });
            await ctx.Response.WriteAsync(body);
        }
        else
        {
            ctx.Response.Redirect("/Home/Error");
        }
    }
}
