using NetVips;

namespace Infrastructure.Shared.Storage;

public interface ICompressProcessor
{
    public void CompressTo(Stream original, Stream target);
}

internal sealed class CompressProcessor : ICompressProcessor
{
    public async void CompressTo(Stream original, Stream target)
    {
        using var image = Image.NewFromStream(original);

        image.WebpsaveStream(target, keep: Enums.ForeignKeep.None);
    }
}
