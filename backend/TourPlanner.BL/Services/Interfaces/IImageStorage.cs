namespace TourPlanner.BL.Services.Interfaces;

public interface IImageStorage
{
    // Returns the public, web-accessible relative path of the stored image.
    Task<string> SaveImageAsync(Stream content, string? contentType, long lengthBytes);
}
