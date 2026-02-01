using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Sqordia.Functions.ExportHandler.Configuration;
using Sqordia.Functions.ExportHandler.Models;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// Service implementation for processing document export jobs (Azure version)
/// </summary>
public class ExportProcessor : IExportProcessor
{
    private readonly ILogger<ExportProcessor> _logger;
    private readonly ExportConfiguration _config;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IEnumerable<IExportGenerator> _generators;

    public ExportProcessor(
        ILogger<ExportProcessor> logger,
        IOptions<ExportConfiguration> config,
        BlobServiceClient blobServiceClient,
        IEnumerable<IExportGenerator> generators)
    {
        _logger = logger;
        _config = config.Value;
        _blobServiceClient = blobServiceClient;
        _generators = generators;
    }

    public async Task<bool> ProcessExportJobAsync(ExportJobMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing export job {JobId} for business plan {BusinessPlanId}, Type: {ExportType}",
                message.JobId,
                message.BusinessPlanId,
                message.ExportType);

            // 1. Find the appropriate generator
            var generator = _generators.FirstOrDefault(g =>
                g.ExportType.Equals(message.ExportType, StringComparison.OrdinalIgnoreCase));

            if (generator == null)
            {
                _logger.LogError("No export generator found for type {ExportType}", message.ExportType);
                return false;
            }

            // 2. Fetch business plan data from database
            var businessPlanData = await FetchBusinessPlanDataAsync(message.BusinessPlanId, cancellationToken);
            if (businessPlanData == null)
            {
                _logger.LogError("Business plan {BusinessPlanId} not found", message.BusinessPlanId);
                return false;
            }

            // 3. Generate the document
            var documentBytes = await generator.GenerateAsync(businessPlanData, message.Language, cancellationToken);
            _logger.LogInformation("Document generated, size: {Size} bytes", documentBytes.Length);

            // 4. Upload to Azure Blob Storage
            var blobUrl = await UploadToBlobStorageAsync(
                message.JobId,
                message.BusinessPlanId,
                generator.FileExtension,
                generator.ContentType,
                documentBytes,
                cancellationToken);

            _logger.LogInformation(
                "Successfully processed export job {JobId} for business plan {BusinessPlanId}. Blob URL: {BlobUrl}",
                message.JobId,
                message.BusinessPlanId,
                blobUrl);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing export job {JobId} for business plan {BusinessPlanId}",
                message.JobId,
                message.BusinessPlanId);
            throw;
        }
    }

    private async Task<BusinessPlanExportData?> FetchBusinessPlanDataAsync(string businessPlanId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(businessPlanId, out var planGuid))
        {
            _logger.LogError("Invalid business plan ID format: {BusinessPlanId}", businessPlanId);
            return null;
        }

        await using var connection = new NpgsqlConnection(_config.DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT
                bp.""Id"",
                bp.""Title"",
                bp.""Description"",
                bp.""Status"",
                bp.""PlanType"",
                bp.""Version"",
                bp.""CreatedAt"",
                bp.""FinalizedAt"",
                bp.""ExecutiveSummary"",
                bp.""ProblemStatement"",
                bp.""Solution"",
                bp.""MarketAnalysis"",
                bp.""CompetitiveAnalysis"",
                bp.""SwotAnalysis"",
                bp.""BusinessModel"",
                bp.""MarketingStrategy"",
                bp.""BrandingStrategy"",
                bp.""OperationsPlan"",
                bp.""ManagementTeam"",
                bp.""FinancialProjections"",
                bp.""FundingRequirements"",
                bp.""RiskAnalysis"",
                bp.""ExitStrategy"",
                bp.""AppendixData"",
                bp.""MissionStatement"",
                bp.""SocialImpact"",
                bp.""BeneficiaryProfile"",
                bp.""GrantStrategy"",
                bp.""SustainabilityPlan"",
                o.""Name"" as ""OrganizationName""
            FROM ""BusinessPlans"" bp
            LEFT JOIN ""Organizations"" o ON bp.""OrganizationId"" = o.""Id""
            WHERE bp.""Id"" = @Id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", planGuid);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BusinessPlanExportData
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Status = reader.GetInt32(reader.GetOrdinal("Status")).ToString(),
            PlanType = reader.GetInt32(reader.GetOrdinal("PlanType")).ToString(),
            Version = reader.GetInt32(reader.GetOrdinal("Version")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            FinalizedAt = reader.IsDBNull(reader.GetOrdinal("FinalizedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("FinalizedAt")),
            OrganizationName = reader.IsDBNull(reader.GetOrdinal("OrganizationName")) ? "Unknown Organization" : reader.GetString(reader.GetOrdinal("OrganizationName")),
            ExecutiveSummary = GetNullableString(reader, "ExecutiveSummary"),
            ProblemStatement = GetNullableString(reader, "ProblemStatement"),
            Solution = GetNullableString(reader, "Solution"),
            MarketAnalysis = GetNullableString(reader, "MarketAnalysis"),
            CompetitiveAnalysis = GetNullableString(reader, "CompetitiveAnalysis"),
            SwotAnalysis = GetNullableString(reader, "SwotAnalysis"),
            BusinessModel = GetNullableString(reader, "BusinessModel"),
            MarketingStrategy = GetNullableString(reader, "MarketingStrategy"),
            BrandingStrategy = GetNullableString(reader, "BrandingStrategy"),
            OperationsPlan = GetNullableString(reader, "OperationsPlan"),
            ManagementTeam = GetNullableString(reader, "ManagementTeam"),
            FinancialProjections = GetNullableString(reader, "FinancialProjections"),
            FundingRequirements = GetNullableString(reader, "FundingRequirements"),
            RiskAnalysis = GetNullableString(reader, "RiskAnalysis"),
            ExitStrategy = GetNullableString(reader, "ExitStrategy"),
            AppendixData = GetNullableString(reader, "AppendixData"),
            MissionStatement = GetNullableString(reader, "MissionStatement"),
            SocialImpact = GetNullableString(reader, "SocialImpact"),
            BeneficiaryProfile = GetNullableString(reader, "BeneficiaryProfile"),
            GrantStrategy = GetNullableString(reader, "GrantStrategy"),
            SustainabilityPlan = GetNullableString(reader, "SustainabilityPlan")
        };
    }

    private static string? GetNullableString(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private async Task<string> UploadToBlobStorageAsync(
        string jobId,
        string businessPlanId,
        string fileExtension,
        string contentType,
        byte[] documentBytes,
        CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = $"exports/{businessPlanId}/{jobId}{fileExtension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var stream = new MemoryStream(documentBytes);
        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);

        // Generate SAS URL for download
        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _config.ContainerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(_config.SasTokenExpirationHours)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        return blobClient.Uri.ToString();
    }
}
