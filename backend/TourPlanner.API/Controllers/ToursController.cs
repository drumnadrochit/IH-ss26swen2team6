using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;

namespace TourPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ToursController : ControllerBase
{
    private readonly ITourService _tourService;
    private readonly IImageStorage _imageStorage;
    private readonly IWeatherService _weatherService;
    private readonly ITourImportExportService _importExportService;

    public ToursController(
        ITourService tourService,
        IImageStorage imageStorage,
        IWeatherService weatherService,
        ITourImportExportService importExportService)
    {
        _tourService = tourService;
        _imageStorage = imageStorage;
        _weatherService = weatherService;
        _importExportService = importExportService;
    }

    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("User ID claim missing"));

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tours = await _tourService.GetToursAsync(UserId);
        return Ok(tours);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(await _tourService.GetToursAsync(UserId));
        var tours = await _tourService.SearchToursAsync(UserId, q);
        return Ok(tours);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var data = await _importExportService.ExportToursAsync(UserId);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"tours-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<TourExportDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new DomainValidationException("Import file contains no tours.");
            var count = await _importExportService.ImportToursAsync(UserId, data);
            return Ok(new { imported = count });
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Invalid JSON file." });
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tour = await _tourService.GetTourByIdAsync(id, UserId);
        return tour == null ? NotFound() : Ok(tour);
    }

    [HttpGet("{id:guid}/weather")]
    public async Task<IActionResult> GetWeather(Guid id)
    {
        var coords = await _tourService.GetTourStartCoordinatesAsync(id, UserId);
        if (coords == null) return NotFound(new { message = "Tour location is unknown." });

        var weather = await _weatherService.GetCurrentWeatherAsync(coords.Value.Lat, coords.Value.Lon);
        return weather == null
            ? StatusCode(StatusCodes.Status502BadGateway, new { message = "Weather service unavailable." })
            : Ok(weather);
    }

    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var path = await _imageStorage.SaveImageAsync(stream, file.ContentType, file.Length);
            var tour = await _tourService.SetTourImageAsync(id, UserId, path);
            return Ok(tour);
        }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (ForbiddenAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request)
    {
        try
        {
            var tour = await _tourService.CreateTourAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = tour.Id }, tour);
        }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequest request)
    {
        try
        {
            var tour = await _tourService.UpdateTourAsync(id, request, UserId);
            return Ok(tour);
        }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (ForbiddenAccessException) { return Forbid(); }
        catch (DomainValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _tourService.DeleteTourAsync(id, UserId);
            return NoContent();
        }
        catch (EntityNotFoundException) { return NotFound(); }
        catch (ForbiddenAccessException) { return Forbid(); }
    }
}
