using System.IO.Compression;
using System.Reflection;
using App.Native;

var assembly = Assembly.GetExecutingAssembly();

var extractDir = Path.Combine(Path.GetTempPath(), "Sast", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(extractDir);

var resourceName = assembly.GetManifestResourceNames()
    .FirstOrDefault(n => n.EndsWith("wwwroot.zip"))
    ?? throw new InvalidOperationException("Embedded wwwroot.zip not found.");

using var zipStream = assembly.GetManifestResourceStream(resourceName)
    ?? throw new InvalidOperationException($"Failed to read embedded resource '{resourceName}'.");

ZipFile.ExtractToDirectory(zipStream, extractDir);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    try { Directory.Delete(extractDir, true); } catch { }
};

var thread = new Thread(() =>
{
    AppNative.Init("SAST Image", 1280, 720, extractDir);
    AppNative.Run();
});
if (OperatingSystem.IsWindows())
    thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
