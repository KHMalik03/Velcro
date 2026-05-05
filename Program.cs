using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using velcro.Data;
using velcro.Hubs;
using velcro.Middleware;
using velcro.Services;
using velcro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Active MVC + vues Razor ; IgnoreCycles évite les boucles infinies en JSON (Card → List → Board → ...)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// FluentValidation : retourne automatiquement un 400 si la requête viole une règle de validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// SQLite via EF Core — la chaîne de connexion est dans appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT : authentification sans session — le client envoie un token signé à chaque requête
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Secret"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true, // rejette les tokens expirés
            ValidateIssuerSigningKey = true, // vérifie la signature HMAC-SHA256
            ValidIssuer              = jwt["Issuer"],
            ValidAudience            = jwt["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(key)
        };
        // SignalR ne peut pas envoyer de header → le token transite par la query string (?access_token=...)
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// Injection de dépendances : Scoped = une instance par requête HTTP
builder.Services.AddScoped<IAuthService,      AuthService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IBoardService,     BoardService>();
builder.Services.AddScoped<IListService,      ListService>();
builder.Services.AddScoped<ICardService,      CardService>();
builder.Services.AddScoped<ICommentService,   CommentService>();

// SignalR : communication temps réel bidirectionnelle via WebSocket
builder.Services.AddSignalR();

// Swagger : interface web pour tester l'API → http://localhost:5265/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TaskBoard API", Version = "v1" });
});

var app = builder.Build();

// En dev : migrations EF appliquées automatiquement au démarrage + Swagger activé
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Pipeline : chaque requête traverse ces middlewares dans l'ordre
app.UseMiddleware<ExceptionMiddleware>(); // intercepte toutes les exceptions non gérées
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();    // sert wwwroot/ (CSS, JS)
app.UseRouting();        // résout quelle route correspond à l'URL
app.UseAuthentication(); // lit le JWT et remplit HttpContext.User
app.UseAuthorization();  // vérifie les permissions ([Authorize])

app.MapStaticAssets();
app.MapControllers();    // routes des API controllers
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapHub<BoardHub>("/hubs/board"); // endpoint WebSocket SignalR

app.Run();
