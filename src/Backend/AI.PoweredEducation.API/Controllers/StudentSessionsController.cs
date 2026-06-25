using System.ComponentModel.DataAnnotations;
using AI.PoweredEducation.Business.StudentSessions.Dtos;
using AI.PoweredEducation.Business.StudentSessions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/student-sessions")]
public sealed class StudentSessionsController : ControllerBase
{
    private const string SessionTokenHeader = "X-Session-Token";
    private readonly IStudentSessionService _service;

    public StudentSessionsController(IStudentSessionService service) => _service = service;

    [HttpPost("join")]
    public async Task<ActionResult<JoinGameResponse>> Join(
        JoinGameRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.JoinAsync(request, cancellationToken));

    [HttpGet("progress")]
    public async Task<ActionResult<StudentProgressResponse>> GetProgress(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetProgressAsync(sessionToken, cancellationToken));

    [HttpPost("tasks/current/start")]
    public async Task<ActionResult<StudentProgressResponse>> StartCurrentTask(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        CancellationToken cancellationToken) =>
        Ok(await _service.StartCurrentTaskAsync(sessionToken, cancellationToken));

    [HttpPost("tasks/current/quiz-answer")]
    public async Task<ActionResult<StudentProgressResponse>> SubmitQuizAnswer(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        SubmitQuizAnswerRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.SubmitQuizAnswerAsync(sessionToken, request, cancellationToken));

    [HttpPost("tasks/current/qr-scan")]
    public async Task<ActionResult<StudentProgressResponse>> ScanQrCode(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        ScanQrCodeRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.ScanQrCodeAsync(sessionToken, request, cancellationToken));

    [HttpPost("tasks/current/gps-location")]
    public async Task<ActionResult<StudentProgressResponse>> CompleteGpsTask(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        CompleteGpsTaskRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.CompleteGpsTaskAsync(sessionToken, request, cancellationToken));

    [HttpPost("tasks/current/timeout")]
    public async Task<ActionResult<StudentProgressResponse>> TimeoutCurrentTask(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        CancellationToken cancellationToken) =>
        Ok(await _service.TimeoutCurrentTaskAsync(sessionToken, cancellationToken));

    [HttpPost("leave")]
    public async Task<ActionResult<ResultResponse>> Leave(
        [Required, FromHeader(Name = SessionTokenHeader)] string sessionToken,
        CancellationToken cancellationToken) =>
        Ok(await _service.LeaveAsync(sessionToken, cancellationToken));
}
