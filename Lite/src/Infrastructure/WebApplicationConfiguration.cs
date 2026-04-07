using Microsoft.AspNetCore.Builder;

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
            builder.Services.AddImageServices();
            builder.Services.AddCategoryServices();
            builder
                .Services.AddUserServices(builder.Configuration)
                .AddJwtAuth(builder.Configuration);
        }
    }
}
