using System.Net.Http;

namespace OurSigner;

public static class OurSignDownloader
{
    private static readonly HttpClient Client = new();

    private const string OurSignUrl =
        "https://github.com/anargihu/OurSign/releases/download/Test/OurSign-0.0.1.ipa";

    public static async Task<string> DownloadAsync(
        IProgress<int>? progress = null)
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OurSigner",
            "OurSign"
        );

        Directory.CreateDirectory(folder);

        string path = Path.Combine(
            folder,
            "OurSign-0.0.1.ipa"
        );

        using HttpResponseMessage response = await Client.GetAsync(
            OurSignUrl,
            HttpCompletionOption.ResponseHeadersRead
        );

        response.EnsureSuccessStatusCode();

        long? total = response.Content.Headers.ContentLength;

        await using Stream input = await response.Content.ReadAsStreamAsync();

        await using FileStream output = new(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        byte[] buffer = new byte[81920];
        long downloaded = 0;
        int read;

        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read));
            downloaded += read;

            if (total.HasValue && total.Value > 0)
            {
                progress?.Report(
                    (int)(downloaded * 100 / total.Value)
                );
            }
        }

        return path;
    }
}