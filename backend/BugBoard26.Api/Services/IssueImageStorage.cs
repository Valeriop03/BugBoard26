using System.Buffers.Binary;

namespace BugBoard26.Api.Services;

public class IssueImageStorage
{
    public const long MaxImageBytes = 5242880;
    private readonly string _directory;

    public IssueImageStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _directory = Path.GetFullPath(configuration["IssueImages:Directory"] ?? "uploads/issue-images", environment.ContentRootPath);
        var webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
        if (_directory.Equals(webRoot, StringComparison.OrdinalIgnoreCase) ||
            _directory.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La cartella delle immagini deve essere esterna alle risorse pubbliche.");
        }
    }

    public async Task<(byte[] Data, string Extension)> ValidateAsync(IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            throw new ArgumentException("Il campo image deve contenere un'immagine non vuota.");
        }
        if (image.Length > MaxImageBytes)
        {
            throw new ArgumentException("L'immagine non puo' superare 5242880 byte (5 MiB).");
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg" or ".jpeg"))
        {
            throw new ArgumentException("Sono consentite soltanto immagini PNG e JPEG (.png, .jpg, .jpeg).");
        }

        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, cancellationToken);
        var data = buffer.ToArray();
        var validPng = data.Length >= 45 &&
            data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
            data.AsSpan(8, 8).SequenceEqual(new byte[] { 0, 0, 0, 13, 73, 72, 68, 82 }) &&
            BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(16, 4)) > 0 &&
            BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(20, 4)) > 0 &&
            data.AsSpan(data.Length - 12).SequenceEqual(new byte[] { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 });
        var validJpeg = data.Length >= 12 && data[0] == 255 && data[1] == 216 && data[2] == 255 &&
            data[^2] == 255 && data[^1] == 217;

        if (data.Length > MaxImageBytes ||
            (extension == ".png" ? !validPng : !validJpeg))
        {
            throw new ArgumentException("Il contenuto del file non corrisponde a un'immagine PNG/JPEG valida con l'estensione indicata.");
        }
        return (data, extension == ".png" ? ".png" : ".jpg");
    }

    public async Task<string> SaveAsync(byte[] data, string extension, CancellationToken cancellationToken)
    {
        if (extension is not (".png" or ".jpg"))
        {
            throw new ArgumentException("Formato immagine non valido.");
        }
        Directory.CreateDirectory(_directory);
        var name = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_directory, name);
        try
        {
            await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await file.WriteAsync(data, cancellationToken);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return name;
    }

    public FileStream? OpenRead(string? name)
    {
        var path = GetPath(name);
        if (path is null)
        {
            return null;
        }
        try
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, true);
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    public void Delete(string? name)
    {
        var path = GetPath(name);
        if (path is not null)
        {
            File.Delete(path);
        }
    }

    public string GetContentType(string name) => Path.GetExtension(name) == ".png" ? "image/png" : "image/jpeg";

    private string? GetPath(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Contains('/') || name.Contains('\\') ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _) ||
            Path.GetExtension(name) is not (".png" or ".jpg"))
        {
            return null;
        }
        return Path.Combine(_directory, name);
    }
}
