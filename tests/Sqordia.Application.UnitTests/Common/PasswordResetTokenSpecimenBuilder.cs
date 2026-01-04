using AutoFixture.Kernel;
using Sqordia.Domain.Entities.Identity;
using System;

namespace Sqordia.Application.UnitTests.Common;

public class PasswordResetTokenSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(PasswordResetToken))
        {
            var userId = (Guid)context.Resolve(typeof(Guid));
            var token = (string)context.Resolve(typeof(string));
            var expiresAt = DateTime.UtcNow.AddHours(1);
            
            return new PasswordResetToken(userId, token, expiresAt);
        }

        return new NoSpecimen();
    }
}
