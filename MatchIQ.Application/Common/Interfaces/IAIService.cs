using MatchIQ.Application.Common.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Common.Interfaces;

public interface IAIService
{
    Task<GeneratedTestDto> GenerateTestAsync(JobOffer offer, TestLanguage language);
    Task<GeneratedQuestionDto> RegenerateQuestionAsync(TestQuestion question, IEnumerable<QuestionChatMessage> history, string adminMessage, TestLanguage language);
    Task<CandidateInsightDto> EvaluateCandidateAsync(JobOffer offer, Match match);
    Task<SubmissionEvaluationDto> EvaluateSubmissionAsync(Test test, TestSubmission submission);
    Task<ProctoringAnalysisDto> AnalyzeProctoringAsync(IEnumerable<ProctoringEvent> events, decimal integrityScore);
}
