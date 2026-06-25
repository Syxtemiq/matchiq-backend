using System.Text;
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
using MatchIQ.Infrastructure.Persistence;
using MatchIQ.Infrastructure.Persistence.Repositories;
using MatchIQ.API.Middlewares;
using MatchIQ.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();

// Current user (lectura de claims del JWT via IHttpContextAccessor)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

builder.Services.AddControllers();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
