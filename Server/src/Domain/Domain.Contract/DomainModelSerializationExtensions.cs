using Domain.ValueObject;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Domain;

public static class DomainModelSerializationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDomainModelJsonSerialization()
        {
            services.AddSerializer(builder =>
                builder.AddJsonSerializer(isSupported: type =>
                {
                    return type.CustomAttributes.Any(attr =>
                        attr.AttributeType.Name.Contains(nameof(OpenJsonConverterAttribute<>))
                    );
                })
            );

            return services;
        }
    }
}
