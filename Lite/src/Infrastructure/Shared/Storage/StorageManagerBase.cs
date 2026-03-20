using Domain.Entity;

namespace Infrastructure.Shared.Storage;

internal abstract class StorageManagerBase(string basePath)
{
    private readonly string basePath = basePath;

    protected internal Stream OpenWrite<TId>(TId id, string? filename = null)
        where TId : ITypedId<TId, long>
    {
        string path = Path.Combine(basePath, id.RelativePath);
        EnsureDirectory(path);
        filename ??= id.Value.ToString();
        path = Path.Combine(path, filename);
        return File.OpenWrite(path);
    }

    protected internal Stream? OpenRead<TId>(TId id, string? mask = null)
        where TId : ITypedId<TId, long>
    {
        mask ??= id.Value.ToString();

        string path = Path.Combine(basePath, id.RelativePath);
        if (!Directory.Exists(path))
            return null;

        // NOTE: Performance: GetFiles is not the most efficient way to get the file.
        string[] files = Directory.GetFiles(path, mask, SearchOption.TopDirectoryOnly);
        if (files.Length <= 0)
            return null;

        var stream = File.OpenRead(files[0]);
        return stream;
    }

    protected internal void Delete<TId>(TId id)
        where TId : ITypedId<TId, long>
    {
        string path = Path.Combine(basePath, id.RelativePath);
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        DeleteIfDirectoryEmpty(Directory.GetParent(path)!.FullName);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static void DeleteIfDirectoryEmpty(string path)
    {
        if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length == 0)
            Directory.Delete(path);
    }
}
