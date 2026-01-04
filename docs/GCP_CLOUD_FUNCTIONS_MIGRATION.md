# GCP Cloud Functions Migration Guide

## Summary

**AWS Lambda functions are NOT directly compatible with GCP Cloud Functions.** They require code adaptation.

## Key Differences

### 1. Event Types
- **AWS Lambda**: Uses `SQSEvent` with `Records[]` containing `Body`, `MessageId`, `ReceiptHandle`
- **GCP Cloud Functions**: Uses `CloudEvent` with `MessagePublishedData` containing Pub/Sub message data

### 2. Function Signatures
- **AWS Lambda**: 
  ```csharp
  public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
  ```
- **GCP Cloud Functions**: 
  ```csharp
  public class FunctionGcp : ICloudEventFunction<MessagePublishedData>
  {
      public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
  }
  ```

### 3. Dependencies
- **AWS**: `Amazon.Lambda.Core`, `Amazon.Lambda.SQSEvents`, `Amazon.Lambda.Serialization.SystemTextJson`
- **GCP**: `Google.Cloud.Functions.Framework`, `Google.Events.Protobuf`

### 4. Message Processing
- **AWS**: Messages come in batches via `SQSEvent.Records`
- **GCP**: Single message per invocation via `MessagePublishedData.Message`

## What We've Created

### GCP-Compatible Function Classes
Created `FunctionGcp.cs` files for all three handlers:
- `src/Lambda/EmailHandler/src/Sqordia.Lambda.EmailHandler/FunctionGcp.cs`
- `src/Lambda/AIGenerationHandler/src/Sqordia.Lambda.AIGenerationHandler/FunctionGcp.cs`
- `src/Lambda/ExportHandler/src/Sqordia.Lambda.ExportHandler/FunctionGcp.cs`

### Updated Project Files
- Added GCP packages to all three function projects
- Updated System.Text.Json to version 8.0.5 (required by Google.Events.Protobuf)
- Updated Terraform to use GCP entry points (`FunctionGcp` instead of `Function`)

## Current Status

### ✅ Completed
1. Created GCP-compatible function classes
2. Added GCP NuGet packages
3. Updated Terraform configuration
4. Fixed package version conflicts

### ⚠️ Pending
1. **CloudEvent Type Issue**: The `CloudEvent` type is not being found. This may require:
   - Additional package reference
   - Different namespace
   - Alternative approach using `IHttpFunction` or direct Pub/Sub message handling

## Next Steps

1. **Resolve CloudEvent Type**:
   - Check if `CloudEvent` is in `Google.Events.Protobuf` namespace
   - Or use alternative interface like `IHttpFunction` with manual Pub/Sub message parsing
   - Or use `Google.Cloud.PubSub.V1` directly

2. **Test GCP Functions**:
   - Build and deploy to GCP
   - Test with actual Pub/Sub messages
   - Verify message processing logic

3. **Update Startup Configuration**:
   - Ensure GCP services (email, storage) are configured instead of AWS
   - Update connection strings for Cloud SQL
   - Configure GCP service accounts

## Alternative Approach

If `ICloudEventFunction` continues to have issues, consider:
- Using `IHttpFunction` and manually parsing Pub/Sub push messages
- Using `Google.Cloud.PubSub.V1` directly to subscribe to topics
- Using Cloud Run instead of Cloud Functions for more control

## Business Logic Reusability

✅ **The business logic (processors) can be reused:**
- `IEmailProcessor.ProcessEmailJobAsync()` - Works with both AWS and GCP
- `IAIGenerationProcessor.ProcessGenerationJobAsync()` - Works with both AWS and GCP  
- `IExportProcessor.ProcessExportJobAsync()` - Works with both AWS and GCP

Only the entry points need to be different for AWS vs GCP.

