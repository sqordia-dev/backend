using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Configuration;
using System.Text;
using System.Text.Json;

namespace WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.WriteIndented = false;
                // Handle circular references (e.g., BusinessPlan -> QuestionnaireResponses -> BusinessPlan)
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth to handle deep object graphs
            });
        services.AddHttpContextAccessor();

        // Add HttpClient for external API calls (e.g., Microsoft Graph API)
        services.AddHttpClient();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = false;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Sqordia API",
                Version = "v1",
                Description = "API for Sqordia Business Plan Management System"
            });

            options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Sqordia API V2",
                Version = "v2",
                Description = "Growth Architect Intelligence Layer - Persona-based questionnaires, Socratic Coach auditing, strategy mapping, and bank-readiness scoring"
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                            configuration["JwtSettings:Secret"] ??
                            throw new InvalidOperationException("JWT Secret must be configured");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        var googleSection = configuration.GetSection("GoogleOAuth");
        var clientId = googleSection["ClientId"];
        var clientSecret = googleSection["ClientSecret"];

        if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret) &&
            !clientId.Contains("TODO", StringComparison.OrdinalIgnoreCase) && 
            !clientSecret.Contains("TODO", StringComparison.OrdinalIgnoreCase))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = "/api/v1/auth/google/callback";
                options.Scope.Add("email");
                options.Scope.Add("profile");
            });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                if (environment.IsDevelopment())
                {
                    // Development: Allow localhost on any port + explicit frontend URL
                    // Get explicit frontend URL from config if provided
                    var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                        ?? configuration["Frontend:BaseUrl"];
                    
                    var allowedOrigins = new List<string>();
                    if (!string.IsNullOrWhiteSpace(frontendBaseUrl))
                    {
                        allowedOrigins.Add(frontendBaseUrl.TrimEnd('/'));
                    }
                    
                    // When using AllowCredentials(), we need to use SetIsOriginAllowed (not WithOrigins)
                    // This allows dynamic origin checking for any localhost port
                    policy.SetIsOriginAllowed(origin => 
                        {
                            if (string.IsNullOrEmpty(origin))
                                return false;
                            
                            // Check explicit allowed origins first
                            if (allowedOrigins.Any(allowed => origin.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                            
                            try
                            {
                                // Parse the origin URI
                                var uri = new Uri(origin);
                                
                                // Check if it's localhost or 127.0.0.1
                                var isLocalhost = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || 
                                                 uri.Host == "127.0.0.1" || 
                                                 uri.Host == "::1" ||
                                                 uri.Host.StartsWith("localhost.", StringComparison.OrdinalIgnoreCase);
                                
                                // Check if it's http or https
                                var isValidScheme = uri.Scheme == "http" || uri.Scheme == "https";
                                
                                var isAllowed = isValidScheme && isLocalhost;
                                
                                return isAllowed;
                            }
                            catch (UriFormatException)
                            {
                                // Invalid URI format
                                return false;
                            }
                            catch
                            {
                                // Any other exception
                                return false;
                            }
                        })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                }
                else
                {
                    // Production: Use FRONTEND_BASE_URL (Option 2 - recommended)
                    var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                        ?? configuration["Frontend:BaseUrl"];

                    var allowedOrigins = new List<string>();

                    if (!string.IsNullOrWhiteSpace(frontendBaseUrl))
                    {
                        // Single frontend URL - trim trailing slashes for exact origin matching
                        allowedOrigins.Add(frontendBaseUrl.TrimEnd('/'));
                    }
                    else
                    {
                        // Fallback: Multiple origins from CORS:AllowedOrigins or CORS__AllowedOrigins
                        var configOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
                            ?? (configuration["CORS__AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            ?? Array.Empty<string>());
                        allowedOrigins.AddRange(configOrigins.Select(o => o.TrimEnd('/')));
                    }

                    if (allowedOrigins.Count > 0)
                    {
                        policy.WithOrigins(allowedOrigins.ToArray())
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .SetPreflightMaxAge(TimeSpan.FromHours(1));
                    }
                    else
                    {
                        // If no origins configured, deny all (fail secure)
                        policy.SetIsOriginAllowed(_ => false)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "fr", "en" };
            options.SetDefaultCulture("fr")
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures);
        });

        return services;
    }

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}

