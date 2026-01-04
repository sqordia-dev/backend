using Google.Cloud.Storage.V1;
using Google;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using System.Text;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// GCP Cloud Storage service configuration
/// </summary>
public class CloudStorageSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

/// <summary>
/// Google Cloud Storage implementation of storage service
/// </summary>
public class CloudStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly CloudStorageSettings _settings;
    private readonly ILogger<CloudStorageService> _logger;

    public CloudStorageService(
        StorageClient storageClient,
        IOptions<CloudStorageSettings> settings,
        ILogger<CloudStorageService> logger)
    {
        _storageClient = storageClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient.UploadObjectAsync(
                bucket: _settings.BucketName,
                objectName: key,
                contentType: contentType,
                source: content,
                cancellationToken: cancellationToken);

            var url = $"https://storage.googleapis.com/{_settings.BucketName}/{key}";
            _logger.LogInformation("File uploaded to Cloud Storage: {Key}", key);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Cloud Storage: {Key}", key);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(content);
        return await UploadFileAsync(key, stream, contentType, cancellationToken);
    }

    public async Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var stream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_settings.BucketName, key, stream, cancellationToken: cancellationToken);
            stream.Position = 0;
            return stream;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found in Cloud Storage: {Key}", key);
            throw new FileNotFoundException($"File '{key}' not found in storage", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Cloud Storage: {Key}", key);
            throw;
        }
    }

    public async Task<byte[]> DownloadFileBytesAsync(string key, CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadFileAsync(key, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_settings.BucketName, key, cancellationToken: cancellationToken);
            _logger.LogInformation("File deleted from Cloud Storage: {Key}", key);
            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found for deletion in Cloud Storage: {Key}", key);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Cloud Storage: {Key}", key);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient.GetObjectAsync(_settings.BucketName, key, cancellationToken: cancellationToken);
            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in Cloud Storage: {Key}", key);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use UrlSigner with default credentials (service account or application default credentials)
            var urlSigner = UrlSigner.FromCredentialFile(null);
            var url = await urlSigner.SignAsync(
                _settings.BucketName,
                key,
                TimeSpan.FromMinutes(expirationMinutes),
                HttpMethod.Get,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Generated pre-signed URL for {Key}, expires in {Minutes} minutes", key, expirationMinutes);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URL for Cloud Storage: {Key}", key);
            // Fallback: return public URL if presigned URL generation fails
            var publicUrl = $"https://storage.googleapis.com/{_settings.BucketName}/{key}";
            _logger.LogWarning("Falling back to public URL: {Url}", publicUrl);
            return publicUrl;
        }
    }
}

