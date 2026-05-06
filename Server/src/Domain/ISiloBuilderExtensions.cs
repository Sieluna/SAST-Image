using Domain.Album;
using Domain.Event;
using Domain.Filters;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Domain;

public static class ISiloBuilderExtensions
{
    extension(ISiloBuilder builder)
    {
        public ISiloBuilder UseDomain<TService1, TService2, TService3>()
            where TService1 : class, IIdUniquenessChecker
            where TService2 : class, IUsernameUniquenessChecker
            where TService3 : class, ICategoryExistenceChecker
        {
            builder.AddOutgoingGrainCallFilter<AccessControlFilter>();
            builder.AddOutgoingGrainCallFilter<EnsureExistsFilter>();
            builder.AddOutgoingGrainCallFilter<EnsureUniqueIdFilter>();
            builder.Services.AddSerializer(b =>
                b.AddJsonSerializer(type => type.Namespace!.StartsWith("Domain"))
            );

            builder.Services.AddSingleton<IIdUniquenessChecker, TService1>();
            builder.Services.AddSingleton<IUsernameUniquenessChecker, TService2>();
            builder.Services.AddSingleton<ICategoryExistenceChecker, TService3>();

            builder.Services.AddDbContextFactory<DomainDbContext>(options =>
                options
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString("Domain"),
                        options =>
                            options.ConfigureDataSource(b =>
                                b.EnableDynamicJson()
                                    .ConfigureJsonOptions(
                                        new() { AllowOutOfOrderMetadataProperties = true }
                                    )
                            )
                    )
                    .UseSnakeCaseNamingConvention()
            );
            builder.Services.AddDbContextFactory<EventDbContext>(option =>
                option
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString("Domain"),
                        options =>
                            options.ConfigureDataSource(b =>
                                b.EnableDynamicJson()
                                    .ConfigureJsonOptions(
                                        new() { AllowOutOfOrderMetadataProperties = true }
                                    )
                            )
                    )
                    .UseSnakeCaseNamingConvention()
            );
            return builder;
        }
    }
}
