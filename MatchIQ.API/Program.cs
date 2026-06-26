using System.Text;
using System.Threading.RateLimiting;
using MatchIQ.Domain.Enums;
using Npgsql;
using Npgsql.NameTranslation;
using MatchIQ.Application.Modules.Admin;
using MatchIQ.Application.Modules.Auth;
using MatchIQ.Application.Modules.Candidate;
using MatchIQ.Application.Modules.Company;
using MatchIQ.Application.Modules.Matching;
using MatchIQ.Application.Modules.Offers;
using MatchIQ.Application.Modules.Tests;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Infrastructure.Auth;
using MatchIQ.Infrastructure.AI;
using MatchIQ.Infrastructure.Email;
using MatchIQ.Infrastructure.Payments;
using MatchIQ.Infrastructure.Persistence;
using MatchIQ.Infrastructure.Persistence.Repositories;
using MatchIQ.API.BackgroundServices;
using MatchIQ.API.Middlewares;
using MatchIQ.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────────────────
var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// Mapeo de enums de PostgreSQL — sin esto EF Core envía text y PostgreSQL rechaza el cast
dataSourceBuilder.MapEnum<UserRole>("user_role_enum");
dataSourceBuilder.MapEnum<Seniority>("seniority_enum");
dataSourceBuilder.MapEnum<Modality>("modality_enum");
dataSourceBuilder.MapEnum<OfferStatus>("offer_status_enum");
dataSourceBuilder.MapEnum<MatchStage>("match_stage_enum");
dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status_enum");
dataSourceBuilder.MapEnum<PaymentStatus>("payment_status_enum");
dataSourceBuilder.MapEnum<QuestionType>("question_type_enum");
dataSourceBuilder.MapEnum<ChatRole>("chat_role_enum");
// EnglishLevel usa labels en mayúsculas ('A1','A2'...) — se preserva el nombre C# tal cual
dataSourceBuilder.MapEnum<EnglishLevel>("english_level_enum", new NpgsqlNullNameTranslator());

var npgsqlDataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(npgsqlDataSource));

// ── Autenticación JWT ─────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login, register, forgot-password, reset-password: 5 intentos/min por IP
    options.AddPolicy("auth-strict", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            Window      = TimeSpan.FromMinutes(1),
            PermitLimit = 5,
            QueueLimit  = 0
        });
    });

    // Verify-email, refresh, google-login: 15 intentos/min por IP
    options.AddPolicy("auth-general", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            Window      = TimeSpan.FromMinutes(1),
            PermitLimit = 15,
            QueueLimit  = 0
        });
    });

    // Pagos: 5 intentos/5min por usuario autenticado (o IP si no hay token)
    options.AddPolicy("payment", context =>
    {
        var key = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.Connection.RemoteIpAddress?.ToString()
                  ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            Window      = TimeSpan.FromMinutes(5),
            PermitLimit = 5,
            QueueLimit  = 0
        });
    });
});

// ── OpenAI SDK ────────────────────────────────────────────────────────────────
builder.Services.AddOpenAIService(settings =>
{
    settings.ApiKey = builder.Configuration["OpenAI:ApiKey"]!;
});

// ── Inyección de dependencias ─────────────────────────────────────────────────
// Application Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CandidateService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<OffersService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<TestEditorService>();
builder.Services.AddScoped<AdminService>();

// Infrastructure Services
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAIService, OpenAIService>();
builder.Services.AddScoped<IOfferParserService, OfferParserService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddHttpClient<IPaymentService, WompiService>();
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();

// Current user (lectura de claims del JWT via IHttpContextAccessor)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Background jobs diarios
builder.Services.AddHostedService<DailyJobsService>();

// ── CORS para Flutter Web ─────────────────────────────────────────────────────
// TODO: configurar origins reales cuando se tenga la URL del frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterWeb", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MatchIQ API",
        Version = "v1",
        Description = "API para la plataforma de matching entre candidatos y empresas."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            []
        }
    });
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Unifica los errores de validación de DTOs con el mismo ApiResponse<T>
        // que usa el resto de la API — sin esto, [ApiController] devolvería ProblemDetails
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                .FirstOrDefault() ?? "Datos de entrada inválidos.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                MatchIQ.API.Common.ApiResponse.Fail(firstError));
        };
    });

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────────────
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MatchIQ API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("FlutterWeb");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
