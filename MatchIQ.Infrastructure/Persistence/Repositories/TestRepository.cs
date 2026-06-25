using MatchIQ.Domain.Entities;
using MatchIQ.Application.Common.Interfaces.Repositories;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

// Implementación del repositorio de tests y preguntas
public class TestRepository : ITestRepository
{
    // TODO: inyectar AppDbContext

    // TODO: GetByOfferIdAsync → _context.Tests
    //                                    .Include(t => t.Questions)
    //                                    .FirstOrDefaultAsync(t => t.OfferId == offerId)

    // TODO: CreateAsync → add + SaveChangesAsync

    // TODO: GetQuestionByIdAsync → _context.TestQuestions.FindAsync(id)

    // TODO: UpdateQuestionAsync → SaveChangesAsync (la entidad ya está tracked por EF)

    // TODO: GetChatHistoryAsync → _context.QuestionChatMessages
    //                                      .Where(m => m.QuestionId == questionId)
    //                                      .OrderBy(m => m.CreatedAt)
    //                                      .ToListAsync()

    // TODO: AddChatMessageAsync → add + SaveChangesAsync
    public async Task<Test?> GetByOfferIdAsync(int offerId)
    {
        throw new NotImplementedException();
    }

    public async Task<Test> CreateAsync(Test test)
    {
        throw new NotImplementedException();
    }

    public async Task<TestQuestion?> GetQuestionByIdAsync(int questionId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateQuestionAsync(TestQuestion question)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<QuestionChatMessage>> GetChatHistoryAsync(int questionId)
    {
        throw new NotImplementedException();
    }

    public async Task AddChatMessageAsync(QuestionChatMessage message)
    {
        throw new NotImplementedException();
    }
}
