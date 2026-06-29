using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Services;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepoMock = null!;
    private IConfiguration _config = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "test-secret-key-minimum-32-chars-long!!",
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
        _authService = new AuthService(_userRepoMock.Object, _config);
    }

    [Test]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _authService.RegisterAsync(new RegisterRequest("testuser", "test@example.com", "password123"));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.AccessToken, Is.Not.Empty);
    }

    [Test]
    public void Register_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var existingUser = new User { Email = "existing@example.com" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("existing@example.com")).ReturnsAsync(existingUser);

        Assert.ThrowsAsync<ConflictException>(() =>
            _authService.RegisterAsync(new RegisterRequest("newuser", "existing@example.com", "password123")));
    }

    [Test]
    public void Register_WithDuplicateUsername_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("existinguser")).ReturnsAsync(new User { Username = "existinguser" });

        Assert.ThrowsAsync<ConflictException>(() =>
            _authService.RegisterAsync(new RegisterRequest("existinguser", "new@example.com", "password123")));
    }

    [Test]
    public void Register_WithShortPassword_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<DomainValidationException>(() =>
            _authService.RegisterAsync(new RegisterRequest("user", "user@test.com", "123")));
    }

    [Test]
    public void Register_WithEmptyUsername_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<DomainValidationException>(() =>
            _authService.RegisterAsync(new RegisterRequest("", "user@test.com", "password123")));
    }

    [Test]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser", PasswordHash = hash };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var result = await _authService.LoginAsync(new LoginRequest("test@example.com", "password123"));

        Assert.That(result.AccessToken, Is.Not.Empty);
        Assert.That(result.UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public void Login_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { Email = "test@example.com", PasswordHash = hash };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.LoginAsync(new LoginRequest("test@example.com", "wrongpassword")));
    }

    [Test]
    public void Login_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.LoginAsync(new LoginRequest("nobody@example.com", "password123")));
    }
}
