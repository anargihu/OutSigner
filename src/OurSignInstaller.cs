using System.Diagnostics;

namespace OurSigner;

public sealed class InstallationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}

public sealed class OurSignInstaller
{
    private static readonly string[] InstallerLocations =
    [
        Path.Combine(
            AppContext.BaseDirectory,
            "ideviceinstaller.exe"
        ),
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "OurSigner",
            "tools",
            "ideviceinstaller.exe"
        )
    ];

    public bool IsAvailable()
    {
        return FindInstaller() != null;
    }

    public async Task<InstallationResult> InstallAsync(
        string ipaPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipaPath))
        {
            return Failure("No IPA was selected.");
        }

        if (!File.Exists(ipaPath))
        {
            return Failure("The selected IPA could not be found.");
        }

        if (!string.Equals(
                Path.GetExtension(ipaPath),
                ".ipa",
                StringComparison.OrdinalIgnoreCase))
        {
            return Failure("The selected file is not an IPA.");
        }

        string? installer = FindInstaller();

        if (installer == null)
        {
            return Failure(
                "The iOS installation backend is not available."
            );
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = installer,
            Arguments = $"\"{ipaPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process
            {
                StartInfo = startInfo
            };

            if (!process.Start())
            {
                return Failure(
                    "The iOS installation backend could not start."
                );
            }

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync(
                    cancellationToken
                );

            Task<string> errorTask =
                process.StandardError.ReadToEndAsync(
                    cancellationToken
                );

            await process.WaitForExitAsync(
                cancellationToken
            );

            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode != 0)
            {
                string message =
                    string.IsNullOrWhiteSpace(error)
                        ? output
                        : error;

                if (string.IsNullOrWhiteSpace(message))
                {
                    message =
                        $"Installer exited with code {process.ExitCode}.";
                }

                return Failure(message.Trim());
            }

            return new InstallationResult
            {
                Success = true,
                Message = string.IsNullOrWhiteSpace(output)
                    ? "OurSign installation completed."
                    : output.Trim()
            };
        }
        catch (OperationCanceledException)
        {
            return Failure("Installation was cancelled.");
        }
        catch (Exception ex)
        {
            return Failure(
                $"Installation failed: {ex.Message}"
            );
        }
    }

    string? FindInstaller()
    {
        foreach (string path in InstallerLocations)
        {
            if (File.Exists(path))
                return path;
        }

        return FindInstallerOnPath();
    }

    static string? FindInstallerOnPath()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "ideviceinstaller.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);

            if (process == null)
                return null;

            string output =
                process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            string[] paths =
                output.Split(
                    Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries
                );

            return paths.FirstOrDefault(
                File.Exists
            );
        }
        catch
        {
            return null;
        }
    }

    static InstallationResult Failure(
        string message)
    {
        return new InstallationResult
        {
            Success = false,
            Message = message
        };
    }
}