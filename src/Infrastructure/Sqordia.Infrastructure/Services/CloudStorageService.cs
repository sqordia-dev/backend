using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage service configuration
/// </summary>
public class AzureStorageSettings
{
    public string ContainerName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

/// <summary>
/// Azure Blob Storage implementation of storage service
/// </summary>
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _settings;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageSettings> settings,
        ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }

    public async Task<string> UploadFileAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

            var url = blobClient.Uri.ToString();
            _logger.LogInformation("File uploaded to Azure Blob Storage: {Key}", key);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {Key}", key);
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
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                _logger.LogWarning("File not found in Azure Blob Storage: {Key}", key);
                throw new FileNotFoundException($"File '{key}' not found in storage");
            }

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob Storage: {Key}", key);
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
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);

            var result = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            if (result.Value)
            {
                _logger.LogInformation("File deleted from Azure Blob Storage: {Key}", key);
            }
            else
            {
                _logger.LogWarning("File not found for deletion in Azure Blob Storage: {Key}", key);
            }
            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {Key}", key);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);
            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in Azure Blob Storage: {Key}", key);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new FileNotFoundException($"File '{key}' not found in storage");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _settings.ContainerName,
                BlobName = key,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(sasBuilder);
            _logger.LogInformation("Generated pre-signed URL for {Key}, expires in {Minutes} minutes", key, expirationMinutes);
            return sasToken.ToString();
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URL for Azure Blob Storage: {Key}", key);
            // Fallback: return blob URL if SAS generation fails
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(key);
            var publicUrl = blobClient.Uri.ToString();
            _logger.LogWarning("Falling back to blob URL: {Url}", publicUrl);
            return publicUrl;
        }
    }
}

