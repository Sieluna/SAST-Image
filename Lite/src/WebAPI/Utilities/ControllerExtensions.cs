using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Utilities;

public static class ControllerExtensions
{
    extension(ControllerBase controller)
    {
        public IActionResult ImageOrNotFound(Stream? image) =>
            image is null ? controller.NotFound() : controller.File(image, "image/*");

        public IActionResult DataOrNotFound(object? data) =>
            data is null ? controller.NotFound() : controller.Ok(data);
    }
}
