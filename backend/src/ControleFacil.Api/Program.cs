using System.Text;
using System.Text.Json.Serialization;
using ControleFacil.Api.Endpoints;
using ControleFacil.Api.Middleware;
using ControleFacil.Api.Services;
using ControleFacil.Application;
using ControleFacil.Application.Interfaces;
using ControleFacil.Infrastructure;
using ControleFacil.Infrastructure.Data;
using ControleFacil.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Handlers de último recurso: cobrem exceções que escapam do pipeline normal de
// middleware/try-catch (ex.: erro numa thread de background, ou uma falha durante o
// próprio startup antes da app existir). Sem isso, um crash chega ao log do Render como
// "Exited with status N" sem nenhum contexto de qual exceção .NET causou a queda —
// Console.Error aqui é deliberado (funciona mesmo antes do DI/ILogger existir e o Render
// captura stdout/stderr do container diretamente).
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Console.Error.WriteLine($"[FATAL] UnhandledException (terminating={e.IsTerminating}): {e.ExceptionObject}");
};
TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Console.Error.WriteLine($"[FATAL] UnobservedTaskException: {e.Exception}");
    e.SetObserved();
};

var builder = WebApplication.CreateBuilder(args);

// O Render injeta a porta via a variável de ambiente PORT e espera a aplicação
// escutar nela. UseUrls() tem prioridade sobre ASPNETCORE_URLS e sobre o
// applicationUrl do launchSettings.json, então só chamamos quando PORT existe de
// verdade — senão o `dotnet run` local perderia o bind em 0.0.0.0:5000 (necessário
// pro app mobile alcançar a API pela rede) e cairia sempre em 8080.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHostedService<DueAlertBackgroundService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer').",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key não configurado");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("forgot-password", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapBankAccountEndpoints();
app.MapTransactionEndpoints();
app.MapDashboardEndpoints();

try
{
    startupLogger.LogInformation("Iniciando migração/seed do banco de dados...");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DbSeeder.SeedAsync(db, passwordHasher);
    }
    startupLogger.LogInformation("Migração/seed concluídos, iniciando o listener HTTP...");
}
catch (Exception ex)
{
    // Se isso falhar (ex.: banco inatingível), o processo vai encerrar de qualquer forma —
    // mas ao menos o log do Render mostra QUAL exceção .NET causou a queda, em vez de só
    // um "Exited with status N" sem contexto nenhum.
    startupLogger.LogCritical(ex, "Falha fatal durante migração/seed do banco no startup.");
    throw;
}

try
{
    app.Run();
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, "Falha fatal não tratada — a aplicação está encerrando.");
    throw;
}
