using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;
using log4net;

namespace TourPlanner.BL.Services;

public class AuthService : IAuthService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(AuthService));
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new DomainValidationException("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new DomainValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainValidationException("Password must be at least 6 characters.");

        var existingEmail = await _userRepo.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            Log.Warn($"Registration rejected: email already registered ({request.Email}).");
            throw new ConflictException("Email already registered.");
        }

        var existingUser = await _userRepo.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            Log.Warn($"Registration rejected: username already taken ({request.Username}).");
            throw new ConflictException("Username already taken.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        await _userRepo.AddAsync(user);
        Log.Info($"User registered: {user.Email}");
        return new AuthResponse(GenerateJwt(user), user.Id, user.Username, user.Email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            Log.Warn($"Failed login attempt for: {request.Email}");
            throw new InvalidCredentialsException("Invalid credentials.");
        }
        Log.Info($"User logged in: {user.Email}");
        return new AuthResponse(GenerateJwt(user), user.Id, user.Username, user.Email);
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
