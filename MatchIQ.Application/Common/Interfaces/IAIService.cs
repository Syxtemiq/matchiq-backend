using MatchIQ.Application.Common.Dtos;
using MatchIQ.Domain.Entities;

namespace MatchIQ.Application.Common.Interfaces;

public interface IAIService
{
    Task<GeneratedTestDto> GenerateTestAsync(JobOffer offer);
    Task<GeneratedQuestionDto> RegenerateQuestionAsync(TestQuestion question, IEnumerable<QuestionChatMessage> history, string adminMessage);
    Task<CandidateInsightDto> EvaluateCandidateAsync(JobOffer offer, Match match);
    Task<SubmissionEvaluationDto> EvaluateSubmissionAsync(Test test, TestSubmission submission);
    Task<ProctoringAnalysisDto> AnalyzeProctoringAsync(IEnumerable<ProctoringEvent> events, decimal integrityScore);
}
