using AutoFixture.Kernel;
using Sqordia.Domain.ValueObjects;
using System;
using System.Reflection;

namespace Sqordia.Application.UnitTests.Common;

public class EmailAddressSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(EmailAddress))
        {
            return new EmailAddress("valid.email@example.com");
        }

        return new NoSpecimen();
    }
}
