using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Query.Database;

namespace Query;

public static class IServiceColletionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds query database context services to the dependency injection container using the specified
        /// configuration.
        /// </summary>
        /// <param name="configuration">IConfigurationRoot["Query"]</param>
        public IServiceCollection AddQueryServices(IConfiguration configuration)
        {
            services.AddDistributedPostgresCache(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("Cache");
                options.CreateIfNotExists = true;
                options.TableName = "cache";
                options.SchemaName = "query";
            });

            services.AddDbContextFactory<QueryDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("Query"))
                    //.UseModel(QueryDbContextModel.Instance)
                    .UseSnakeCaseNamingConvention()
            );
            services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
            services.AddHostedService<QueryService>();

            return services;
        }
    }
}
