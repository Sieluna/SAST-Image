using Domain.Shared;
using Storage;

namespace WebAPI.Utilities;

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

internal static class IEndpointRouteBuilderExtensions
{
    extension<T>(T app)
        where T : IEndpointRouteBuilder, IApplicationBuilder
    {
        public IEndpointRouteBuilder Endpoints
        {
            get
            {
                var group = app.MapGroup("/api");
                group.ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json");
                group.ProducesValidationProblem(
                    StatusCodes.Status400BadRequest,
                    "application/problem+json"
                );

                return group;
            }
        }
    }
}
