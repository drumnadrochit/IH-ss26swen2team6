using Microsoft.Extensions.Options;
using NUnit.Framework;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class FileImageStorageTests
{
    private string _tempDir = null!;
    private FileImageStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "tourplanner-tests-" + Guid.NewGuid());
        _storage = new FileImageStorage(Options.Create(new StorageOptions { BasePath = _tempDir }));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    public async Task SaveImageAsync_WithValidJpeg_SavesFileAndReturnsPath()
    {
        using var content = new MemoryStream([1, 2, 3, 4]);

        var path = await _storage.SaveImageAsync(content, "image/jpeg", content.Length);

        Assert.That(path, Does.StartWith("/uploads/"));
        Assert.That(path, Does.EndWith(".jpg"));
        var fileName = Path.GetFileName(path);
        Assert.That(File.Exists(Path.Combine(_tempDir, fileName)), Is.True);
    }

    [Test]
    public void SaveImageAsync_WithUnsupportedType_ThrowsArgumentException()
    {
        using var content = new MemoryStream([1, 2, 3]);
        Assert.ThrowsAsync<DomainValidationException>(() => _storage.SaveImageAsync(content, "application/pdf", content.Length));
    }

    [Test]
    public void SaveImageAsync_WithOversizedFile_ThrowsArgumentException()
    {
        using var content = new MemoryStream([1]);
        Assert.ThrowsAsync<DomainValidationException>(() => _storage.SaveImageAsync(content, "image/png", 6 * 1024 * 1024));
    }

    [Test]
    public void SaveImageAsync_WithMissingContentType_ThrowsArgumentException()
    {
        using var content = new MemoryStream([1]);
        Assert.ThrowsAsync<DomainValidationException>(() => _storage.SaveImageAsync(content, null, content.Length));
    }
}
