namespace MatchIQ.Infrastructure.Persistence;

// DbContext principal de la aplicación
// Configura todas las entidades, relaciones, enums y constraints
// Los enums se guardan como strings en Postgres (HasConversion<string>)
public class AppDbContext // : DbContext
{
    // TODO: constructor con DbContextOptions

    // TODO: DbSet<User> Users
    // TODO: DbSet<CandidateProfile> CandidateProfiles
    // TODO: DbSet<CompanyProfile> CompanyProfiles
    // TODO: DbSet<Category> Categories
    // TODO: DbSet<Skill> Skills
    // TODO: DbSet<CandidateCategory> CandidateCategories
    // TODO: DbSet<CandidateSkill> CandidateSkills
    // TODO: DbSet<JobOffer> JobOffers
    // TODO: DbSet<OfferCategory> OfferCategories
    // TODO: DbSet<OfferSkill> OfferSkills
    // TODO: DbSet<Match> Matches
    // TODO: DbSet<Test> Tests
    // TODO: DbSet<TestQuestion> TestQuestions
    // TODO: DbSet<QuestionChatMessage> QuestionChatMessages
    // TODO: DbSet<TestSubmission> TestSubmissions
    // TODO: DbSet<RefreshToken> RefreshTokens

    // TODO: OnModelCreating(ModelBuilder modelBuilder)
    //       configurar relaciones, unique constraints, conversiones de enums
    //       los enums como string para que Postgres los guarde legibles
    //       índices en: users.email, matches(offer_id, candidate_id), refresh_tokens.token
}
