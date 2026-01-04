using AutoFixture;
using AutoFixture.Kernel;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Domain.UnitTests.Common;

public class EmailAddressSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(EmailAddress))
        {
            var fixture = new Fixture();
            var validEmails = new[]
            {
                "test@example.com",
                "user@domain.co.uk",
                "admin@company.org",
                "support@service.net",
                "info@business.com"
            };
            
            var randomEmail = validEmails[fixture.Create<int>() % validEmails.Length];
            return new EmailAddress(randomEmail);
        }

        return new NoSpecimen();
    }
}
