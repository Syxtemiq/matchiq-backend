// MatchIQ API - Entry point
// Configura todos los servicios, middlewares y la pipeline de la aplicación

using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Autenticación JWT + Google OAuth ─────────────────────────────────────────
// TODO: builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//           .AddJwtBearer(options => { ... })
//           .AddGoogle(options => { ... })

// ── Inyección de dependencias ─────────────────────────────────────────────────
// Application Services
// TODO: builder.Services.AddScoped<AuthService>()
// TODO: builder.Services.AddScoped<CandidateService>()
// TODO: builder.Services.AddScoped<CompanyService>()
// TODO: builder.Services.AddScoped<OffersService>()
// TODO: builder.Services.AddScoped<MatchingService>()
// TODO: builder.Services.AddScoped<TestService>()
// TODO: builder.Services.AddScoped<TestEditorService>()
// TODO: builder.Services.AddScoped<AdminService>()

// Infrastructure Services
// TODO: builder.Services.AddScoped<IAIService, OpenAIService>()
// TODO: builder.Services.AddScoped<IOfferParserService, OfferParserService>()
// TODO: builder.Services.AddScoped<IEmailService, MailKitEmailService>()
// TODO: builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>()
// TODO: builder.Services.AddScoped<IMatchRepository, MatchRepository>()
// TODO: builder.Services.AddScoped<ITestRepository, TestRepository>()

// ── CORS para Flutter Web ─────────────────────────────────────────────────────
// TODO: builder.Services.AddCors(options => { ... })

// ── Swagger ───────────────────────────────────────────────────────────────────
// TODO: builder.Services.AddEndpointsApiExplorer()
// TODO: builder.Services.AddSwaggerGen()

builder.Services.AddControllers();

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────────────
// TODO: app.UseMiddleware<ErrorHandlingMiddleware>()
// TODO: app.UseMiddleware<CurrentUserMiddleware>()
// TODO: app.UseCors(...)
// TODO: app.UseAuthentication()
// TODO: app.UseAuthorization()
// TODO: app.UseSwagger() / app.UseSwaggerUI()
app.MapControllers();

app.Run();
