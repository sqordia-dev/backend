using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAISettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed default AI provider settings
            migrationBuilder.Sql(@"
                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"")
                SELECT
                    gen_random_uuid(),
                    'AI.ActiveProvider',
                    'OpenAI',
                    'AI',
                    'Active AI provider (OpenAI, Claude, or Gemini)',
                    false,
                    1, -- Config
                    1, -- String
                    false,
                    true,
                    NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'AI.ActiveProvider'
                );

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"")
                SELECT
                    gen_random_uuid(),
                    'AI.FallbackProviders',
                    '[""Claude"", ""Gemini""]',
                    'AI',
                    'Fallback AI providers in order of preference',
                    false,
                    1, -- Config
                    3, -- Json
                    false,
                    true,
                    NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'AI.FallbackProviders'
                );

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"")
                SELECT
                    gen_random_uuid(),
                    'AI.OpenAI.Model',
                    'gpt-4o',
                    'AI',
                    'OpenAI model name',
                    false,
                    1, -- Config
                    1, -- String
                    false,
                    false,
                    NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'AI.OpenAI.Model'
                );

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"")
                SELECT
                    gen_random_uuid(),
                    'AI.Claude.Model',
                    'claude-3-5-sonnet-20241022',
                    'AI',
                    'Claude model name',
                    false,
                    1, -- Config
                    1, -- String
                    false,
                    false,
                    NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'AI.Claude.Model'
                );

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"")
                SELECT
                    gen_random_uuid(),
                    'AI.Gemini.Model',
                    'gemini-1.5-pro',
                    'AI',
                    'Gemini model name',
                    false,
                    1, -- Config
                    1, -- String
                    false,
                    false,
                    NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'AI.Gemini.Model'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove AI settings
            migrationBuilder.Sql(@"
                DELETE FROM ""Settings""
                WHERE ""Key"" IN (
                    'AI.ActiveProvider',
                    'AI.FallbackProviders',
                    'AI.OpenAI.Model',
                    'AI.Claude.Model',
                    'AI.Gemini.Model'
                );
            ");
        }
    }
}
