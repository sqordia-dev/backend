using Microsoft.Extensions.Options;

namespace WebAPI.Configuration;

public class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret))
        {
            return ValidateOptionsResult.Fail("JWT Secret is required. Set it via JwtSettings:Secret in appsettings.json or JWT_SECRET environment variable.");
        }

        if (options.Secret.Length < 32)
        {
            return ValidateOptionsResult.Fail("JWT Secret must be at least 32 characters long for security.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("JWT Issuer is required. Set it via JwtSettings:Issuer in appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("JWT Audience is required. Set it via JwtSettings:Audience in appsettings.json.");
        }

        return ValidateOptionsResult.Success;
    }
}

