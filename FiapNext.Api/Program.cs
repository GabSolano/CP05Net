using System.Text;
using FiapNext.Api.Endpoints;
using FiapNext.Api.Middlewares;
using FiapNext.Api.Telemetry;
using FiapNext.Application;
using FiapNext.Application.Common.Metrics;
using FiapNext.Infrastructure;
using FiapNext.Infrastructure.Context;
using FiapNext.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Logging Estruturado com Serilog
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. OpenTelemetry (Métricas & Tracing)
// ==========================================
builder.Services.AddSingleton<FiapNextTelemetry>();
builder.Services.AddSingleton<IFiapNextMetrics>(sp => sp.GetRequiredService<FiapNextTelemetry>());

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(FiapNextTelemetry.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(FiapNextTelemetry.ActivitySourceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

// ==========================================
// 3. Camadas de Aplicação e Infraestrutura
// ==========================================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ==========================================
// 4. Segurança e Autorização (JWT & RBAC)
// ==========================================
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Policy exigida pelo enunciado: RequireAuthorization("SomenteAdmin")
    options.AddPolicy("SomenteAdmin", policy => policy.RequireRole("Admin"));
});

// ==========================================
// 5. Documentação Swagger / OpenAPI com Bearer Auth
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP NEXT - API RESTful de Gestão de Trabalhos e Grupos",
        Version = "v1",
        Description = "API corporativa desenvolvida para o festival FIAP NEXT com Clean Architecture, NoSQL (MongoDB), HATEOAS Nível 3, JWT & RBAC e Observabilidade.\n\n" +
                      "Integrantes (Turma 2TDSPV):\n" +
                      "- Gabriel Costa Solano (RM562325)\n" +
                      "- Leandro Guarido (RM561760)\n" +
                      "- Kaiky Pereira Rodrigues (RM564578)",
        Contact = new OpenApiContact
        {
            Name = "Gabriel Costa Solano | Leandro Guarido | Kaiky Pereira Rodrigues - 2TDSPV",
            Email = "rm562325@fiap.com.br"
        }
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// ==========================================
// 6. Pipeline de Middlewares
// ==========================================
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FIAP NEXT API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz (/)
    });
}

app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 7. Health Checks (/health)
// ==========================================
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
            checkedAt = DateTime.UtcNow,
            dependencies = report.Entries.Select(e => new
            {
                service = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds + "ms"
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// ==========================================
// 8. Mapeamento dos Endpoints (Minimal APIs)
// ==========================================
app.MapAuthEndpoints();
app.MapTrabalhosEndpoints();
app.MapGruposEndpoints();

// Inicialização opcional de seed no MongoDB
try
{
    using var scope = app.Services.CreateScope();
    var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    // Não trava a inicialização se mongo remoto ainda não configurado
    _ = Task.Run(async () =>
    {
        try { await mongoContext.SeedInitialDataAsync(); }
        catch { /* Banco inicializado sob demanda */ }
    });
}
catch { /* Tolerância a falhas na inicialização */ }

app.Run();

// Necessário para testes de integração ou WebApplicationFactory caso desejado
public partial class Program { }
