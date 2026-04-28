using Domain.Shared;

namespace WebAPI.Utilities;

public static class IFormFileExtensions
{
    extension(IFormFile file)
    {
        public async Task<ImageFile> GetAsync(CancellationToken cancellationToken = default)
        {
            string temp = Path.GetTempFileName();
            await using var stream = new FileStream(temp, FileMode.OpenOrCreate);
            await file.CopyToAsync(stream, cancellationToken);
            return new ImageFile() { Value = temp };
        }
    }
}
