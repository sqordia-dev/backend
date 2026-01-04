using Sqordia.Application.Common.Interfaces;
using System.Collections.Concurrent;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// In-memory storage service for testing and development environments
/// </summary>
public class InMemoryStorageService : IStorageService
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();
    private readonly ConcurrentDictionary<string, string> _contentTypes = new();

    public Task<string> UploadFileAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        
        _storage[key] = bytes;
        _contentTypes[key] = contentType;
        
        // Return a mock URL
        return Task.FromResult($"https://mock-storage.local/{key}");
    }

    public Task<string> UploadFileAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        _storage[key] = content;
        _contentTypes[key] = contentType;
        
        // Return a mock URL
        return Task.FromResult($"https://mock-storage.local/{key}");
    }

    public Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(key, out var bytes))
        {
            throw new FileNotFoundException($"File not found: {key}");
        }
        
        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task<byte[]> DownloadFileBytesAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(key, out var bytes))
        {
            throw new FileNotFoundException($"File not found: {key}");
        }
        
        return Task.FromResult(bytes);
    }

    public Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        var removed = _storage.TryRemove(key, out _);
        _contentTypes.TryRemove(key, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(key));
    }

    public Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        if (!_storage.ContainsKey(key))
        {
            throw new FileNotFoundException($"File not found: {key}");
        }
        
        // Return a mock pre-signed URL
        return Task.FromResult($"https://mock-storage.local/{key}?expires={DateTime.UtcNow.AddMinutes(expirationMinutes):O}");
    }
}

