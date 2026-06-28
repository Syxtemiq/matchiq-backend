namespace MatchIQ.Application.Modules.Tests.Dtos;

public class StartTestResponseDto
{
    public int SubmissionId { get; set; }
    public TestDto Test { get; set; } = null!;
}
