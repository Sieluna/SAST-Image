using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.ValueObject;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class OpenJsonConverterAttribute<TObject, TValue>()
    : JsonConverterAttribute(typeof(OpenJsonConverter<TObject, TValue>))
    where TObject : IValueObject<TObject, TValue> { }

file sealed class OpenJsonConverter<TObject, TValue> : JsonConverter<TObject>
    where TObject : IValueObject<TObject, TValue>
{
    public override TObject? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        if (TObject.TryCreateNew(value!, out var newObject) == false)
            throw new DomainValueObjectInvalidException(value?.ToString());

        return newObject;
    }

    public override void Write(Utf8JsonWriter writer, TObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TObject);
}

[GenerateSerializer]
[Alias("invalid_value_object_exception")]
internal sealed class DomainValueObjectInvalidException(string? value)
    : Exception($"Invalid value object: {value ?? "null"}");
