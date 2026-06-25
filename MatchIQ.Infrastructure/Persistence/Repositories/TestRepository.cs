using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Domain.Entities;
using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

public class TestRepository : ITestRepository
{
    private readonly AppDbContext _context;

    public TestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Test?> GetByOfferIdAsync(int offerId) =>
        await _context.Tests
            .Include(t => t.TestQuestions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(t => t.OfferId == offerId);

    public async Task<Test> CreateAsync(Test test)
    {
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return test;
    }

    public async Task<TestQuestion?> GetQuestionByIdAsync(int questionId) =>
        await _context.TestQuestions
            .Include(q => q.QuestionChatMessages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(q => q.Id == questionId);

    public async Task UpdateQuestionAsync(TestQuestion question)
    {
        question.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<QuestionChatMessage>> GetChatHistoryAsync(int questionId) =>
        await _context.QuestionChatMessages
            .Where(m => m.QuestionId == questionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

    public async Task AddChatMessageAsync(QuestionChatMessage message)
    {
        _context.QuestionChatMessages.Add(message);
        await _context.SaveChangesAsync();
    }
}
