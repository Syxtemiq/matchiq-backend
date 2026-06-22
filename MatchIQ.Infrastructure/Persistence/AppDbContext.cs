using MatchIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Infrastructure.Persistence;

// DbContext principal de la aplicación
// Configura todas las entidades, relaciones, enums y constraints
// Los enums se guardan como strings en Postgres (HasConversion<string>)
// El mapeo de tablas/columnas en snake_case y las constraints siguen 1 a 1 a DBContext.md
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CandidateCategory> CandidateCategories => Set<CandidateCategory>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OfferCategory> OfferCategories => Set<OfferCategory>();
    public DbSet<OfferSkill> OfferSkills => Set<OfferSkill>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();
    public DbSet<QuestionChatMessage> QuestionChatMessages => Set<QuestionChatMessage>();
    public DbSet<TestSubmission> TestSubmissions => Set<TestSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder);
        ConfigureProfiles(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigurePricingAndOffers(modelBuilder);
        ConfigureMatching(modelBuilder);
        ConfigureTests(modelBuilder);
    }

    // =========================================================================
    // USERS / AUTH
    // =========================================================================
    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.Role).HasColumnName("role").HasConversion<string>().IsRequired();
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.GoogleId).HasColumnName("google_id").HasMaxLength(255);
            e.Property(x => x.PictureUrl).HasColumnName("picture_url");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.GoogleId).IsUnique().HasFilter("google_id IS NOT NULL");
        });

        modelBuilder.Entity<EmailVerification>(e =>
        {
            e.ToTable("email_verifications");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(6).IsRequired();
            e.Property(x => x.Used).HasColumnName("used").HasDefaultValue(false);
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => new { x.UserId, x.Used }).HasDatabaseName("idx_email_verifications_user");

            e.HasOne(x => x.User)
                .WithMany(u => u.EmailVerifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("password_reset_tokens");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.Property(x => x.Token).HasColumnName("token").IsRequired();
            e.Property(x => x.Used).HasColumnName("used").HasDefaultValue(false);
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_password_reset_token");

            e.HasOne(x => x.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.Property(x => x.Token).HasColumnName("token").IsRequired();
            e.Property(x => x.Revoked).HasColumnName("revoked").HasDefaultValue(false);
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_refresh_tokens_token");

            e.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // =========================================================================
    // CANDIDATE / COMPANY PROFILES
    // =========================================================================
    private static void ConfigureProfiles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandidateProfile>(e =>
        {
            e.ToTable("candidate_profiles");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.Property(x => x.ExperienceYears).HasColumnName("experience_years");
            e.Property(x => x.Seniority).HasColumnName("seniority").HasConversion<string>();
            e.Property(x => x.EnglishLevel).HasColumnName("english_level").HasConversion<string>();
            e.Property(x => x.GithubLink).HasColumnName("github_link");
            e.Property(x => x.ProfilePhotoUrl).HasColumnName("profile_photo_url");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.UserId).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("ck_candidate_profiles_experience_years", "experience_years >= 0"));

            e.HasOne(x => x.User)
                .WithOne(u => u.CandidateProfile)
                .HasForeignKey<CandidateProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyProfile>(e =>
        {
            e.ToTable("company_profiles");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.Property(x => x.CompanyName).HasColumnName("company_name").HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.UserId).IsUnique();

            e.HasOne(x => x.User)
                .WithOne(u => u.CompanyProfile)
                .HasForeignKey<CompanyProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // =========================================================================
    // CATEGORIES / SKILLS (catálogo + pivots de candidato)
    // =========================================================================
    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();

            e.HasIndex(x => new { x.Name, x.CategoryId }).IsUnique();

            e.HasOne(x => x.Category)
                .WithMany(c => c.Skills)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CandidateCategory>(e =>
        {
            e.ToTable("candidate_categories");
            e.Property(x => x.CandidateId).HasColumnName("candidate_id").IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();

            e.HasIndex(x => new { x.CandidateId, x.CategoryId }).IsUnique();
            e.HasIndex(x => x.CandidateId).HasDatabaseName("idx_candidate_categories");

            e.HasOne(x => x.CandidateProfile)
                .WithMany(cp => cp.CandidateCategories)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Category)
                .WithMany(c => c.CandidateCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CandidateSkill>(e =>
        {
            e.ToTable("candidate_skills");
            e.Property(x => x.CandidateId).HasColumnName("candidate_id").IsRequired();
            e.Property(x => x.SkillId).HasColumnName("skill_id").IsRequired();
            e.Property(x => x.Level).HasColumnName("level");

            e.HasIndex(x => new { x.CandidateId, x.SkillId }).IsUnique();
            e.HasIndex(x => x.CandidateId).HasDatabaseName("idx_candidate_skills");
            e.ToTable(t => t.HasCheckConstraint("ck_candidate_skills_level", "level BETWEEN 1 AND 5"));

            e.HasOne(x => x.CandidateProfile)
                .WithMany(cp => cp.CandidateSkills)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Skill)
                .WithMany(s => s.CandidateSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // =========================================================================
    // PRICING TIERS / JOB OFFERS / PAYMENTS / OFFER PIVOTS
    // =========================================================================
    private static void ConfigurePricingAndOffers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PricingTier>(e =>
        {
            e.ToTable("pricing_tiers");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            e.Property(x => x.MinCandidates).HasColumnName("min_candidates").IsRequired();
            e.Property(x => x.MaxCandidates).HasColumnName("max_candidates").IsRequired();
            e.Property(x => x.PriceCop).HasColumnName("price_cop").HasPrecision(12, 2).IsRequired();
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_pricing_tiers_min_candidates", "min_candidates > 0");
                t.HasCheckConstraint("ck_pricing_tiers_max_candidates", "max_candidates >= min_candidates");
                t.HasCheckConstraint("ck_pricing_tiers_price_cop", "price_cop >= 0");
            });
        });

        modelBuilder.Entity<JobOffer>(e =>
        {
            e.ToTable("job_offers");
            e.Property(x => x.CompanyId).HasColumnName("company_id").IsRequired();
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Salary).HasColumnName("salary").HasPrecision(12, 2);
            e.Property(x => x.Modality).HasColumnName("modality").HasConversion<string>().IsRequired();
            e.Property(x => x.MinExperienceYears).HasColumnName("min_experience_years");
            e.Property(x => x.RequiredEnglishLevel).HasColumnName("required_english_level").HasConversion<string>();
            e.Property(x => x.PositionsAvailable).HasColumnName("positions_available").HasDefaultValue(1);
            e.Property(x => x.TierId).HasColumnName("tier_id").IsRequired();
            e.Property(x => x.CandidatesToTest).HasColumnName("candidates_to_test");
            e.Property(x => x.CandidatesTestedCount).HasColumnName("candidates_tested_count").HasDefaultValue(0);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasDefaultValue(Domain.Enums.OfferStatus.PendingPayment);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.PaidAt).HasColumnName("paid_at").HasColumnType("timestamp");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp");
            e.Property(x => x.TestSentAt).HasColumnName("test_sent_at").HasColumnType("timestamp");

            e.HasIndex(x => x.TierId).HasDatabaseName("idx_offers_tier_id");
            e.HasIndex(x => x.CompanyId).HasDatabaseName("idx_offers_company_id");
            e.HasIndex(x => x.Status).HasDatabaseName("idx_offers_status");
            e.HasIndex(x => x.Status)
                .HasDatabaseName("idx_offers_test_sent_status")
                .HasFilter("status = 'TestSent'");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_job_offers_min_experience_years", "min_experience_years >= 0");
                t.HasCheckConstraint("ck_job_offers_positions_available", "positions_available > 0");
                t.HasCheckConstraint("ck_job_offers_candidates_to_test", "candidates_to_test IS NULL OR candidates_to_test > 0");
                t.HasCheckConstraint("ck_job_offers_candidates_tested_count", "candidates_tested_count >= 0");
            });

            e.HasOne(x => x.CompanyProfile)
                .WithMany(cp => cp.JobOffers)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.PricingTier)
                .WithMany(t => t.JobOffers)
                .HasForeignKey(x => x.TierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            e.Property(x => x.TierId).HasColumnName("tier_id").IsRequired();
            e.Property(x => x.StripePaymentIntentId).HasColumnName("stripe_payment_intent_id").HasMaxLength(255);
            e.Property(x => x.StripeCheckoutSessionId).HasColumnName("stripe_checkout_session_id").HasMaxLength(255);
            e.Property(x => x.AmountCop).HasColumnName("amount_cop").HasPrecision(12, 2).IsRequired();
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasDefaultValue(Domain.Enums.PaymentStatus.Pending);
            e.Property(x => x.PaidAt).HasColumnName("paid_at").HasColumnType("timestamp");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.OfferId).IsUnique().HasDatabaseName("idx_payments_offer_id");
            e.HasIndex(x => x.Status).HasDatabaseName("idx_payments_status");
            e.HasIndex(x => x.StripePaymentIntentId).IsUnique().HasDatabaseName("idx_payments_stripe_intent");
            e.HasIndex(x => x.StripeCheckoutSessionId).IsUnique();

            e.HasOne(x => x.JobOffer)
                .WithOne(jo => jo.Payment)
                .HasForeignKey<Payment>(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.PricingTier)
                .WithMany(t => t.Payments)
                .HasForeignKey(x => x.TierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OfferCategory>(e =>
        {
            e.ToTable("offer_categories");
            e.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();

            e.HasIndex(x => new { x.OfferId, x.CategoryId }).IsUnique();
            e.HasIndex(x => x.OfferId).HasDatabaseName("idx_offer_categories");

            e.HasOne(x => x.JobOffer)
                .WithMany(jo => jo.OfferCategories)
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Category)
                .WithMany(c => c.OfferCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfferSkill>(e =>
        {
            e.ToTable("offer_skills");
            e.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            e.Property(x => x.SkillId).HasColumnName("skill_id").IsRequired();

            e.HasIndex(x => new { x.OfferId, x.SkillId }).IsUnique();
            e.HasIndex(x => x.OfferId).HasDatabaseName("idx_offer_skills");

            e.HasOne(x => x.JobOffer)
                .WithMany(jo => jo.OfferSkills)
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Skill)
                .WithMany(s => s.OfferSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // =========================================================================
    // MATCHES
    // =========================================================================
    private static void ConfigureMatching(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(e =>
        {
            e.ToTable("matches");
            e.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            e.Property(x => x.CandidateId).HasColumnName("candidate_id").IsRequired();
            e.Property(x => x.MatchPercentage).HasColumnName("match_percentage").HasPrecision(5, 2);
            e.Property(x => x.AdjustedScore).HasColumnName("adjusted_score").HasPrecision(5, 2);
            e.Property(x => x.AiFeedback).HasColumnName("ai_feedback").HasColumnType("jsonb");
            e.Property(x => x.Stage).HasColumnName("stage").HasConversion<string>().HasDefaultValue(Domain.Enums.MatchStage.Matched);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => new { x.OfferId, x.CandidateId }).IsUnique();
            e.HasIndex(x => x.OfferId).HasDatabaseName("idx_matches_offer_id");
            e.HasIndex(x => x.CandidateId).HasDatabaseName("idx_matches_candidate_id");
            e.HasIndex(x => new { x.OfferId, x.Stage }).HasDatabaseName("idx_matches_stage");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_matches_match_percentage", "match_percentage BETWEEN 0 AND 100");
                t.HasCheckConstraint("ck_matches_adjusted_score", "adjusted_score BETWEEN 0 AND 100");
            });

            e.HasOne(x => x.JobOffer)
                .WithMany(jo => jo.Matches)
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CandidateProfile)
                .WithMany(cp => cp.Matches)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // =========================================================================
    // TESTS / PREGUNTAS / CHAT / SUBMISSIONS
    // =========================================================================
    private static void ConfigureTests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>(e =>
        {
            e.ToTable("tests");
            e.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            e.Property(x => x.TimeLimitMinutes).HasColumnName("time_limit_minutes").HasDefaultValue(30);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.OfferId).IsUnique().HasDatabaseName("idx_tests_offer_id");
            e.ToTable(t => t.HasCheckConstraint("ck_tests_time_limit_minutes", "time_limit_minutes > 0"));

            e.HasOne(x => x.JobOffer)
                .WithOne(jo => jo.Test)
                .HasForeignKey<Test>(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestQuestion>(e =>
        {
            e.ToTable("test_questions");
            e.Property(x => x.TestId).HasColumnName("test_id").IsRequired();
            e.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();
            e.Property(x => x.QuestionType).HasColumnName("question_type").HasConversion<string>().IsRequired();
            e.Property(x => x.QuestionText).HasColumnName("question_text").IsRequired();
            e.Property(x => x.Explanation).HasColumnName("explanation");
            e.Property(x => x.IsGorilla).HasColumnName("is_gorilla").HasDefaultValue(false);
            e.Property(x => x.GorillaHint).HasColumnName("gorilla_hint");
            e.Property(x => x.OptionsJson).HasColumnName("options_json").HasColumnType("jsonb");
            e.Property(x => x.CorrectAnswer).HasColumnName("correct_answer").HasMaxLength(1);
            e.Property(x => x.Language).HasColumnName("language").HasMaxLength(50);
            e.Property(x => x.FunctionSignature).HasColumnName("function_signature");
            e.Property(x => x.ExampleInput).HasColumnName("example_input");
            e.Property(x => x.ExpectedBehavior).HasColumnName("expected_behavior");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => new { x.TestId, x.OrderIndex }).IsUnique();
            e.HasIndex(x => x.TestId).HasDatabaseName("idx_questions_test_id");
            e.ToTable(t => t.HasCheckConstraint("ck_test_questions_order_index", "order_index > 0"));

            e.HasOne(x => x.Test)
                .WithMany(t => t.TestQuestions)
                .HasForeignKey(x => x.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionChatMessage>(e =>
        {
            e.ToTable("question_chat_messages");
            e.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
            e.Property(x => x.Role).HasColumnName("role").HasConversion<string>().IsRequired();
            e.Property(x => x.Content).HasColumnName("content").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.QuestionId).HasDatabaseName("idx_chat_messages_question_id");

            e.HasOne(x => x.TestQuestion)
                .WithMany(q => q.QuestionChatMessages)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestSubmission>(e =>
        {
            e.ToTable("test_submissions");
            e.Property(x => x.TestId).HasColumnName("test_id").IsRequired();
            e.Property(x => x.CandidateId).HasColumnName("candidate_id").IsRequired();
            e.Property(x => x.AnswersJson).HasColumnName("answers_json").HasColumnType("jsonb");
            e.Property(x => x.Score).HasColumnName("score").HasPrecision(5, 2);
            e.Property(x => x.Feedback).HasColumnName("feedback");
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasDefaultValue(Domain.Enums.SubmissionStatus.Pending);
            e.Property(x => x.StartedAt).HasColumnName("started_at").HasColumnType("timestamp");
            e.Property(x => x.SubmittedAt).HasColumnName("submitted_at").HasColumnType("timestamp");
            e.Property(x => x.AiEvaluatedAt).HasColumnName("ai_evaluated_at").HasColumnType("timestamp");
            e.Property(x => x.Deadline).HasColumnName("deadline").HasColumnType("timestamp");

            e.HasIndex(x => new { x.TestId, x.CandidateId }).IsUnique();
            e.HasIndex(x => x.TestId).HasDatabaseName("idx_submissions_test_id");
            e.HasIndex(x => x.CandidateId).HasDatabaseName("idx_submissions_candidate_id");
            e.HasIndex(x => x.Deadline)
                .HasDatabaseName("idx_submissions_deadline")
                .HasFilter("status = 'Pending'");

            e.ToTable(t => t.HasCheckConstraint("ck_test_submissions_score", "score BETWEEN 0 AND 100"));

            e.HasOne(x => x.Test)
                .WithMany(t => t.TestSubmissions)
                .HasForeignKey(x => x.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CandidateProfile)
                .WithMany(cp => cp.TestSubmissions)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
