using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;

namespace TourPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (InvalidCredentialsException ex) { return Unauthorized(new { message = ex.Message }); }
    }
}
