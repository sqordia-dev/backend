using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class OBNLControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;
    private readonly Fixture _fixture;
    private readonly IServiceScope _scope;

    public OBNLControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("OBNLTestDb");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateOBNLPlan_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "To serve the community",
            Vision = "A thriving community",
            Values = "Compassion, Integrity",
            FundingRequirements = 250000m,
            FundingPurpose = "Program expansion",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Community engagement plan",
            ImpactMeasurement = "Impact measurement framework",
            SustainabilityStrategy = "Sustainability strategy"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/obnl/plans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOBNLPlan_ExistingPlan_ShouldReturnPlan()
    {
        // Arrange
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/obnl/plans/{plan.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Mission");
    }

    [Fact]
    public async Task GetOBNLPlan_NonExistentPlan_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/obnl/plans/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOBNLPlansByOrganization_ExistingPlans_ShouldReturnPlans()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        
        var plan1 = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Mission 1",
            Vision = "Vision 1",
            Values = "Values 1",
            FundingRequirements = 100000,
            FundingPurpose = "Purpose 1",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "111111111",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Engagement 1",
            ImpactMeasurement = "Measurement 1",
            SustainabilityStrategy = "Strategy 1",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        var plan2 = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Foundation",
            Mission = "Mission 2",
            Vision = "Vision 2",
            Values = "Values 2",
            FundingRequirements = 200000,
            FundingPurpose = "Purpose 2",
            LegalStructure = "Foundation",
            RegistrationNumber = "222222222",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Trustees",
            BoardComposition = "7 members",
            StakeholderEngagement = "Engagement 2",
            ImpactMeasurement = "Measurement 2",
            SustainabilityStrategy = "Strategy 2",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.AddRange(plan1, plan2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/obnl/organizations/{organizationId}/plans");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Mission 1");
        content.Should().Contain("Mission 2");
    }

    [Fact]
    public async Task UpdateOBNLPlan_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Original Mission",
            Vision = "Original Vision",
            Values = "Original Values",
            FundingRequirements = 100000,
            FundingPurpose = "Original Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Original Engagement",
            ImpactMeasurement = "Original Measurement",
            SustainabilityStrategy = "Original Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var updateRequest = new
        {
            Mission = "Updated Mission",
            Vision = "Updated Vision",
            Values = "Updated Values",
            FundingRequirements = 150000m,
            FundingPurpose = "Updated Purpose"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/obnl/plans/{plan.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteOBNLPlan_ExistingPlan_ShouldReturnNoContent()
    {
        // Arrange
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/obnl/plans/{plan.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AnalyzeCompliance_ExistingPlan_ShouldReturnComplianceAnalysis()
    {
        // Arrange
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/v1/obnl/plans/{plan.Id}/compliance/analyze", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _scope?.Dispose();
    }
}
