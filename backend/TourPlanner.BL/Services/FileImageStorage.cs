using Microsoft.Extensions.Options;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;

namespace TourPlanner.BL.Services;

// Images are stored externally on the filesystem (not in the database), under a base
// directory that is configurable via appsettings/env vars (Storage:BasePath).
public class FileImageStorage : IImageStorage
{
    private static readonly Dictionary<string, string> AllowedTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly string _basePath;

    public FileImageStorage(IOptions<StorageOptions> options)
    {
        _basePath = options.Value.BasePath;
    }

    public async Task<string> SaveImageAsync(Stream content, string? contentType, long lengthBytes)
    {
        if (contentType == null || !AllowedTypes.TryGetValue(contentType, out var extension))
            throw new DomainValidationException("Unsupported image type. Allowed: JPEG, PNG, WEBP.");
        if (lengthBytes <= 0 || lengthBytes > MaxBytes)
            throw new DomainValidationException("Image size must be between 1 byte and 5 MB.");

        Directory.CreateDirectory(_basePath);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_basePath, fileName);

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream);

        return $"/uploads/{fileName}";
    }
}
