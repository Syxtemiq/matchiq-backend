using MatchIQ.Application.Common.Dtos;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Domain.Entities;

namespace MatchIQ.Infrastructure.AI;

public class OpenAIService : IAIService
{
    // TODO: inyectar IConfiguration para leer OPENAI_API_KEY y OPENAI_MODEL

    public async Task<GeneratedTestDto> GenerateTestAsync(JobOffer offer)
    {
        throw new NotImplementedException();
    }

    public async Task<GeneratedQuestionDto> RegenerateQuestionAsync(TestQuestion question, IEnumerable<QuestionChatMessage> history, string adminMessage)
    {
        throw new NotImplementedException();
    }

    public async Task<CandidateInsightDto> EvaluateCandidateAsync(JobOffer offer, Match match)
    {
        throw new NotImplementedException();
    }

    public async Task<SubmissionEvaluationDto> EvaluateSubmissionAsync(Test test, TestSubmission submission)
    {
        throw new NotImplementedException();
    }
}
