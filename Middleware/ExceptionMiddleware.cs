using System.Text.Json;

namespace velcro.Middleware;

// Middleware : composant qui s'intercale dans le pipeline HTTP pour traiter toutes les requêtes
public class ExceptionMiddleware
{
    private readonly RequestDelegate    _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx); // passe au middleware suivant (ou au controller)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception non gérée sur {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        // Mappe chaque type d'exception vers le bon code HTTP
        var (status, message) = ex switch
        {
            KeyNotFoundException e        => (StatusCodes.Status404NotFound,           e.Message),
            UnauthorizedAccessException e => (StatusCodes.Status403Forbidden,          e.Message),
            InvalidOperationException e   => (StatusCodes.Status400BadRequest,         e.Message), // ex: email déjà pris
            ArgumentException e           => (StatusCodes.Status400BadRequest,         e.Message),
            _                             => (StatusCodes.Status500InternalServerError,
                                              _env.IsDevelopment() ? ex.ToString() : "Une erreur interne s'est produite.")
        };

        // Pour les routes /api/* → réponse JSON ; pour les vues MVC → redirection
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode  = status;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
        }
        else
        {
            ctx.Response.Redirect("/Home/Error");
        }
    }
}
