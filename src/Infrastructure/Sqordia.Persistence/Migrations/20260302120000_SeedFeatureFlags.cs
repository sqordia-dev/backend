using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedFeatureFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed predefined feature flags with JSON metadata
            // SettingType: 3 = FeatureFlag
            // DataType: 3 = Json

            // AI Features
            migrationBuilder.Sql(@"
                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:AIGenerationEnabled',
                    '{""isEnabled"":true,""description"":""Enable AI-powered content generation"",""category"":""AI"",""tags"":[""ai"",""core""],""type"":1,""state"":0}',
                    'Features', 'Enable AI-powered content generation', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:AIGenerationEnabled');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:UseClaudeAsDefault',
                    '{""isEnabled"":false,""description"":""Use Claude as the default AI provider"",""category"":""AI"",""tags"":[""ai"",""provider""],""type"":1,""state"":0}',
                    'Features', 'Use Claude as the default AI provider', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:UseClaudeAsDefault');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:UseGeminiAsDefault',
                    '{""isEnabled"":false,""description"":""Use Gemini as the default AI provider"",""category"":""AI"",""tags"":[""ai"",""provider""],""type"":1,""state"":0}',
                    'Features', 'Use Gemini as the default AI provider', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:UseGeminiAsDefault');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:EnableAIFallbackMode',
                    '{""isEnabled"":true,""description"":""Enable fallback to alternative AI providers"",""category"":""AI"",""tags"":[""ai"",""fallback""],""type"":1,""state"":0}',
                    'Features', 'Enable fallback to alternative AI providers', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:EnableAIFallbackMode');
            ");

            // Export Features
            migrationBuilder.Sql(@"
                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:ExportToPDF',
                    '{""isEnabled"":true,""description"":""Enable PDF export functionality"",""category"":""Export"",""tags"":[""export"",""pdf""],""type"":1,""state"":0}',
                    'Features', 'Enable PDF export functionality', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:ExportToPDF');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:ExportToWord',
                    '{""isEnabled"":true,""description"":""Enable Word document export"",""category"":""Export"",""tags"":[""export"",""word""],""type"":1,""state"":0}',
                    'Features', 'Enable Word document export', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:ExportToWord');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:ExportToExcel',
                    '{""isEnabled"":false,""description"":""Enable Excel spreadsheet export"",""category"":""Export"",""tags"":[""export"",""excel""],""type"":1,""state"":0}',
                    'Features', 'Enable Excel spreadsheet export', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:ExportToExcel');
            ");

            // Premium Features
            migrationBuilder.Sql(@"
                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:AdvancedAnalytics',
                    '{""isEnabled"":false,""description"":""Enable advanced analytics dashboard"",""category"":""Premium"",""tags"":[""premium"",""analytics""],""type"":1,""state"":0}',
                    'Features', 'Enable advanced analytics dashboard', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:AdvancedAnalytics');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:CollaborativeEditing',
                    '{""isEnabled"":false,""description"":""Enable real-time collaborative editing"",""category"":""Premium"",""tags"":[""premium"",""collaboration""],""type"":1,""state"":0}',
                    'Features', 'Enable real-time collaborative editing', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:CollaborativeEditing');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:MultipleBusinessPlans',
                    '{""isEnabled"":true,""description"":""Allow multiple business plans per user"",""category"":""Premium"",""tags"":[""premium"",""plans""],""type"":1,""state"":0}',
                    'Features', 'Allow multiple business plans per user', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:MultipleBusinessPlans');

                INSERT INTO ""Settings"" (""Id"", ""Key"", ""Value"", ""Category"", ""Description"",
                                        ""IsPublic"", ""SettingType"", ""DataType"", ""IsEncrypted"",
                                        ""IsCritical"", ""Created"", ""IsDeleted"")
                SELECT gen_random_uuid(), 'Features:UnlimitedRevisions',
                    '{""isEnabled"":false,""description"":""Enable unlimited document revisions"",""category"":""Premium"",""tags"":[""premium"",""revisions""],""type"":1,""state"":0}',
                    'Features', 'Enable unlimited document revisions', true, 3, 3, false, false, NOW(), false
                WHERE NOT EXISTS (SELECT 1 FROM ""Settings"" WHERE ""Key"" = 'Features:UnlimitedRevisions');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded feature flags
            migrationBuilder.Sql(@"
                DELETE FROM ""Settings""
                WHERE ""Key"" IN (
                    'Features:AIGenerationEnabled',
                    'Features:UseClaudeAsDefault',
                    'Features:UseGeminiAsDefault',
                    'Features:EnableAIFallbackMode',
                    'Features:ExportToPDF',
                    'Features:ExportToWord',
                    'Features:ExportToExcel',
                    'Features:AdvancedAnalytics',
                    'Features:CollaborativeEditing',
                    'Features:MultipleBusinessPlans',
                    'Features:UnlimitedRevisions'
                );
            ");
        }
    }
}
