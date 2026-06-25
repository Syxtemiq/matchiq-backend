using MatchIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<EmailVerification> EmailVerifications { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<CompanyProfile> CompanyProfiles { get; }
    DbSet<Category> Categories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<CandidateCategory> CandidateCategories { get; }
    DbSet<CandidateSkill> CandidateSkills { get; }
    DbSet<PricingTier> PricingTiers { get; }
    DbSet<JobOffer> JobOffers { get; }
    DbSet<Payment> Payments { get; }
    DbSet<OfferCategory> OfferCategories { get; }
    DbSet<OfferSkill> OfferSkills { get; }
    DbSet<Match> Matches { get; }
    DbSet<Test> Tests { get; }
    DbSet<TestQuestion> TestQuestions { get; }
    DbSet<QuestionChatMessage> QuestionChatMessages { get; }
    DbSet<TestSubmission> TestSubmissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}