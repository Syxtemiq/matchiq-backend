using MatchIQ.Domain.Entities;

namespace MatchIQ.Domain.Interfaces.Repositories;

// Contrato de acceso a datos para tests y preguntas
public interface ITestRepository
{
    Task<Test?> GetByOfferIdAsync(int offerId);
    Task<Test> CreateAsync(Test test);
    Task<TestQuestion?> GetQuestionByIdAsync(int questionId);
    Task UpdateQuestionAsync(TestQuestion question);
    Task<IEnumerable<QuestionChatMessage>> GetChatHistoryAsync(int questionId);
    Task AddChatMessageAsync(QuestionChatMessage message);
}
