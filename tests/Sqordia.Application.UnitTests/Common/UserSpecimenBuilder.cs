using AutoFixture.Kernel;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;
using System;

namespace Sqordia.Application.UnitTests.Common;

public class UserSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(User))
        {
            var firstName = (string)context.Resolve(typeof(string));
            var lastName = (string)context.Resolve(typeof(string));
            var email = new EmailAddress("test@example.com");
            var userName = (string)context.Resolve(typeof(string));
            
            return new User(firstName, lastName, email, userName);
        }

        return new NoSpecimen();
    }
}
