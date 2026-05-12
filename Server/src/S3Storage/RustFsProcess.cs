using System.Diagnostics;

namespace S3Storage;

public sealed class RustFsProcess : IDisposable
{
    private Process? _process;
    private readonly RustFsOptions _options;

    public int Port => _options.Port;

    public RustFsProcess(RustFsOptions options)
    {
        _options = options;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        var rid = GetRuntimeIdentifier();
        var exeName = rid.StartsWith("win") ? "rustfs.exe" : "rustfs";
        var exePath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", exeName);

        if (!File.Exists(exePath))
            throw new FileNotFoundException($"RustFS binary not found at {exePath}");

        // chmod on Unix
        if (!rid.StartsWith("win"))
            Process.Start("chmod", $"+x \"{exePath}\"")?.WaitForExit(1000);

        var dataDir = Path.Combine(_options.DataDirectory, "data");
        Directory.CreateDirectory(dataDir);

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"server --address \"0.0.0.0:{_options.Port}\" --data-dir \"{dataDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        _process.OutputDataReceived += (_, e) => Log(e.Data);
        _process.ErrorDataReceived += (_, e) => Log(e.Data);

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Wait for server to be ready
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!cts.IsCancellationRequested)
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var response = await http.GetAsync($"http://127.0.0.1:{_options.Port}/health", ct);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch { }

            await Task.Delay(500, ct);
        }

        throw new TimeoutException("RustFS server did not start within 30 seconds");
    }

    public void Dispose()
    {
        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            _process.Dispose();
        }
    }

    private static void Log(string? message)
    {
        if (message is not null)
            Debug.WriteLine($"[RustFS] {message}");
    }

    private static string GetRuntimeIdentifier()
    {
        if (OperatingSystem.IsLinux()) return "linux-x64";  // TODO: detect arm64
        if (OperatingSystem.IsMacOS()) return "osx-x64";    // TODO: detect arm64
        if (OperatingSystem.IsWindows()) return "win-x64";
        throw new PlatformNotSupportedException();
    }
}

public sealed record RustFsOptions
{
    public int Port { get; init; } = 9000;
    public string DataDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "rustfs-data");
}
