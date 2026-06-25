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
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Autenticación JWT + Google OAuth ─────────────────────────────────────────
// TODO: builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//           .AddJwtBearer(options => { ... })
//           .AddGoogle(options => { ... })

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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAIService, OpenAIService>();
builder.Services.AddScoped<IOfferParserService, OfferParserService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();

// ── CORS para Flutter Web ─────────────────────────────────────────────────────
// TODO: builder.Services.AddCors(options => { ... })

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

    // Esquema de seguridad JWT Bearer
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
// TODO: app.UseMiddleware<ErrorHandlingMiddleware>()
// TODO: app.UseMiddleware<CurrentUserMiddleware>()
// TODO: app.UseCors(...)
// TODO: app.UseAuthentication()
// TODO: app.UseAuthorization()

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MatchIQ API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:{port}/
    });
}

app.MapControllers();

app.Run();
