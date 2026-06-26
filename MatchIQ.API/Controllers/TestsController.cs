using MatchIQ.API.Common;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Tests;
using MatchIQ.Application.Modules.Tests.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/tests")]
public class TestsController : ControllerBase
{
    private readonly TestService _testService;
    private readonly TestEditorService _testEditorService;
    private readonly ICurrentUserService _currentUser;

    public TestsController(
        TestService testService,
        TestEditorService testEditorService,
        ICurrentUserService currentUser)
    {
        _testService = testService;
        _testEditorService = testEditorService;
        _currentUser = currentUser;
    }

    // ── Empresa ───────────────────────────────────────────────────────────────────

    [HttpPost("{offerId:int}/generate")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> GenerateTest(int offerId)
    {
        var test = await _testService.GenerateTestAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(test, "Test generado correctamente."));
    }

    [HttpPost("{offerId:int}/regenerate")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> RegenerateTest(int offerId)
    {
        var test = await _testService.GenerateTestAsync(offerId, _currentUser.UserId, forceRegenerate: true);
        return Ok(ApiResponse.Ok(test, "Test regenerado correctamente."));
    }

    [HttpGet("{offerId:int}")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> GetFullTest(int offerId)
    {
        var test = await _testService.GetFullTestAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(test));
    }

    [HttpGet("questions/{questionId:int}/chat")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> GetChatHistory(int questionId)
    {
        var history = await _testEditorService.GetChatHistoryAsync(questionId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(history));
    }

    [HttpPost("questions/{questionId:int}/chat")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> SendChatMessage(int questionId, [FromBody] SendChatMessageDto dto)
    {
        var result = await _testEditorService.SendMessageAsync(questionId, _currentUser.UserId, dto.Message);
        return Ok(ApiResponse.Ok(result));
    }

    // ── Candidato ─────────────────────────────────────────────────────────────────

    [HttpGet("{offerId:int}/candidate/preview")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetTestPreview(int offerId)
    {
        var preview = await _testService.GetTestPreviewAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(preview));
    }

    [HttpPost("{offerId:int}/candidate/start")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> StartTest(int offerId)
    {
        var test = await _testService.StartTestAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(test));
    }

    [HttpPost("{testId:int}/submit")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> SubmitAnswers(int testId, [FromBody] SubmitAnswersDto dto)
    {
        var result = await _testService.SubmitAnswersAsync(testId, _currentUser.UserId, dto);
        return Ok(ApiResponse.Ok(result, "Respuestas enviadas y evaluadas correctamente."));
    }

    [HttpGet("{testId:int}/result")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetSubmissionResult(int testId)
    {
        var result = await _testService.GetSubmissionResultAsync(testId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(result));
    }
}
