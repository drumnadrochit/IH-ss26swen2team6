using System.ComponentModel.DataAnnotations;

namespace TourPlanner.BL.DTOs;

public record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Username,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(string AccessToken, Guid UserId, string Username, string Email);
