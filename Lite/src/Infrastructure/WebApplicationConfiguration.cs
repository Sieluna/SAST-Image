using Microsoft.AspNetCore.Builder;
using Query;
using Storage;
using Storage.Albums;
using Storage.Images;

namespace Infrastructure;

public static class WebApplicationConfiguration
{
    extension(WebApplication app)
    {
        public void RunBackend()
        {
            app.UseCors(cors =>
                cors.AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials()
            );

            app.UseResponseCaching();

            app.UseExceptionHandler(op => { });

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

public static class WebApplicationBuilderConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddServices()
        {
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddAlbumServices();
            builder.Services.AddCategoryServices();
            builder
                .Services.AddUserServices(builder.Configuration)
                .AddJwtAuth(builder.Configuration);

            builder.Services.AddQueryServices(builder.Configuration);
            builder.Services.AddStorageServices<AlbumAvailabilityChecker, ImageAvailabilityChecker>(
                builder.Configuration
            );
        }
    }
}
