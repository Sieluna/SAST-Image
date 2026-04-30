using Domain.Entity;
using Domain.Shared;

namespace WebAPI.Utilities;

public static class ModelBindExtensions
{
    extension<TObject, TValue>(TObject)
        where TObject : IValueObject<TObject, TValue>, IFactoryConstructor<TObject, TValue>
    {
        public static TObject Bind(TValue value)
        {
            if (TObject.TryCreateNew(value, out var model) == false)
            {
                throw new DomainModelInvalidException(value?.ToString());
            }
            return model;
        }
    }
}
