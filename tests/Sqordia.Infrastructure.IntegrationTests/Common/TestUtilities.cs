using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Infrastructure.IntegrationTests.Common;

public class IntegrationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public IntegrationTestFixture()
    {
        var services = new ServiceCollection();
        
        // Add in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add services
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddAutoMapper(typeof(Sqordia.Application.ConfigureServices));

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(EmailAddress to, string subject, string body)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(IEnumerable<EmailAddress> to, string subject, string body)
    {
        return Task.CompletedTask;
    }

    public Task SendHtmlEmailAsync(EmailAddress to, string subject, string htmlBody)
    {
        return Task.CompletedTask;
    }

    public Task SendHtmlEmailAsync(IEnumerable<EmailAddress> to, string subject, string htmlBody)
    {
        return Task.CompletedTask;
    }

    public Task SendWelcomeWithVerificationAsync(string email, string firstName, string lastName, string userName, string verificationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string userName, string resetToken)
    {
        return Task.CompletedTask;
    }

    public Task SendAccountLockedAsync(string email, string userName, DateTime lockedUntil)
    {
        return Task.CompletedTask;
    }

    public Task SendLoginAlertAsync(string email, string userName, string ipAddress, DateTime loginTime)
    {
        return Task.CompletedTask;
    }

    public Task SendAccountLockoutNotificationAsync(string email, string firstName, TimeSpan lockoutDuration, DateTime lockedAt)
    {
        return Task.CompletedTask;
    }

    public Task SendOrganizationInvitationAsync(string email, string invitationToken, string? message = null)
    {
        return Task.CompletedTask;
    }

    public Task SendBusinessPlanGeneratedAsync(string email, string userName, string businessPlanId, string businessPlanTitle)
    {
        return Task.CompletedTask;
    }
}
