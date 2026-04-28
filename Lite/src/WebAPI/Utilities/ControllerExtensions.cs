using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Storage;

namespace WebAPI.Utilities;

public static class ControllerExtensions
{
    extension(ControllerBase controller)
    {
        public IActionResult ImageOrNotFound(ImageFile? image) =>
            image is null
                ? controller.NotFound()
                : controller.PhysicalFile(
                    image.Value.Value,
                    contentType: "image/*",
                    fileDownloadName: image.Value.DownloadName
                );

        public IActionResult DataOrNotFound(object? data) =>
            data is null ? controller.NotFound() : controller.Ok(data);
    }
}

internal static class ImageFileExtension
{
    extension(ImageFile file)
    {
        public string DownloadName
        {
            get
            {
                string filename = Path.GetFileName(file.Value);
                return Path.ChangeExtension(filename, file.Extension);
            }
        }
    }
}
