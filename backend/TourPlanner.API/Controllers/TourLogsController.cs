using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;

namespace TourPlanner.API.Controllers;

[ApiController]
[Route("api/tours/{tourId:guid}/logs")]
[Authorize]
public class TourLogsController : ControllerBase
{
    private readonly ITourLogService _logService;

    public TourLogsController(ITourLogService logService) => _logService = logService;

    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("User ID claim missing"));

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid tourId)
    {
        try
        {
            var logs = await _logService.GetLogsAsync(tourId, UserId);
            return Ok(logs);
        }
        catch (EntityNotFoundException) { return NotFound(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid tourId, [FromBody] CreateTourLogRequest request)
    {
        try
        {
            var log = await _logService.CreateLogAsync(tourId, request, UserId);
            return StatusCode(201, log);
        }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{logId:guid}")]
    public async Task<IActionResult> Update(Guid tourId, Guid logId, [FromBody] UpdateTourLogRequest request)
    {
        try
        {
            var log = await _logService.UpdateLogAsync(tourId, logId, request, UserId);
            return Ok(log);
        }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (ForbiddenAccessException) { return Forbid(); }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{logId:guid}")]
    public async Task<IActionResult> Delete(Guid tourId, Guid logId)
    {
        try
        {
            await _logService.DeleteLogAsync(tourId, logId, UserId);
            return NoContent();
        }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (ForbiddenAccessException) { return Forbid(); }
    }
}
