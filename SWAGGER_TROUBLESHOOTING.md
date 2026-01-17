# Swagger Troubleshooting Guide

## Issue: "Failed to load API definition" / "Failed to fetch /swagger/v1/swagger.json"

### Common Causes and Solutions

#### 1. **Application Not Starting**
   - **Check**: Look at the Visual Studio Output window or Debug console for startup errors
   - **Solution**: Ensure the application starts without exceptions
   - **Verify**: Check that you see "Sqordia application started successfully" in the logs

#### 2. **Database Connection Issues**
   - **Check**: Ensure PostgreSQL is running (Docker container `sqordia-db-dev` or local PostgreSQL)
   - **Solution**: 
     - If using Docker: `docker-compose -f docker-compose.dev.yml up -d sqordia-db`
     - Verify connection string in `appsettings.json` matches your database setup
   - **Note**: The app should still start even if database connection fails (migrations are non-blocking)

#### 3. **Port Mismatch**
   - **Check**: Your `launchSettings.json` shows the app runs on `https://localhost:5241`
   - **Solution**: Make sure you're accessing Swagger at: `https://localhost:5241/swagger`
   - **Note**: If you see a different port in the console output, use that port instead

#### 4. **HTTPS Certificate Issues**
   - **Check**: Browser might show certificate warnings
   - **Solution**: 
     - Click "Advanced" → "Proceed to localhost" (for development)
     - Or access via HTTP: `http://localhost:5242/swagger` (if HTTP is enabled)

#### 5. **Swagger Endpoint Not Found**
   - **Check**: Try accessing the JSON directly: `https://localhost:5241/swagger/v1/swagger.json`
   - **Solution**: If this returns 404, check that:
     - `AddSwaggerGen` is called in `ServiceCollectionExtensions.cs`
     - `UseSwagger()` and `UseSwaggerUI()` are called in `WebApplicationExtensions.cs`

### Quick Fix Steps

1. **Clean and Rebuild**:
   ```
   dotnet clean
   dotnet build
   ```

2. **Check Application Startup**:
   - Run the application in Visual Studio
   - Check the Output window for any errors
   - Look for "Sqordia application started successfully" message

3. **Verify Swagger Endpoint**:
   - Once the app is running, try: `https://localhost:5241/swagger/v1/swagger.json`
   - This should return JSON, not an error

4. **Check Browser Console**:
   - Open browser DevTools (F12)
   - Check the Network tab for failed requests
   - Look for CORS errors or 404 errors

### Expected Behavior

When the application starts successfully:
- You should see logs indicating the app is listening on the configured ports
- Accessing `https://localhost:5241/swagger` should show the Swagger UI
- The Swagger UI should automatically load the API definition from `/swagger/v1/swagger.json`

### If Still Not Working

1. Check Visual Studio Output window for specific error messages
2. Verify all NuGet packages are restored: `dotnet restore`
3. Ensure no port conflicts (another app using port 5241)
4. Try running from command line: `dotnet run --project src/WebAPI/WebAPI.csproj`
