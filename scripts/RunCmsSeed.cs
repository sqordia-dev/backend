// Simple C# script to run CMS seed SQL
// Usage: dotnet script RunCmsSeed.cs
// Or: dotnet-script RunCmsSeed.cs

using System;
using Npgsql;
using System.IO;

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? "Host=localhost;Port=5433;Database=SqordiaDb;Username=sqordia;Password=SqordiaDev2025!;SSL Mode=Prefer;Trust Server Certificate=true";

var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "cms-seed-data.sql");
if (!File.Exists(scriptPath))
{
    // Try parent directory
    scriptPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "cms-seed-data.sql");
}

if (!File.Exists(scriptPath))
{
    Console.WriteLine($"Script not found: {scriptPath}");
    Environment.Exit(1);
}

Console.WriteLine($"Executing: {scriptPath}");
Console.WriteLine($"Connection: {connectionString.Replace("Password=", "Password=***")}");

var sqlScript = File.ReadAllText(scriptPath);

using var connection = new NpgsqlConnection(connectionString);
connection.Open();
Console.WriteLine("Connected successfully!");

using var command = new NpgsqlCommand(sqlScript, connection);
command.CommandTimeout = 300; // 5 minutes

try
{
    var rowsAffected = command.ExecuteNonQuery();
    Console.WriteLine($"✅ Script executed successfully! Rows affected: {rowsAffected}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
    Environment.Exit(1);
}
