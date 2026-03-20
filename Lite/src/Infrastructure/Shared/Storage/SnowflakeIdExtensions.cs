using System.Globalization;
using Domain.Entity;
using IdGen;

namespace Infrastructure.Shared.Storage;

internal static class SnowflakeIdExtensions
{
    extension<TId>(TId id)
        where TId : ITypedId<TId, long>
    {
        public string RelativePath
        {
            get
            {
                var epoch = IdGeneratorOptions.DefaultEpoch;
                long timestamp = (id.Value >> 22);

                if (timestamp <= 0)
                {
                    return id.Value.ToString(CultureInfo.InvariantCulture);
                }

                var dateTime = epoch.AddMilliseconds(timestamp);

                return dateTime.ToString($"yyyy/MM/dd/{id.Value}", CultureInfo.InvariantCulture);
            }
        }
    }
}
