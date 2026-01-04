using AutoFixture.Kernel;
using Sqordia.Domain.Entities.Identity;
using System;

namespace Sqordia.Application.UnitTests.Common;

public class RefreshTokenSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(RefreshToken))
        {
            var userId = (Guid)context.Resolve(typeof(Guid));
            var token = (string)context.Resolve(typeof(string));
            var expiresAt = DateTime.UtcNow.AddHours(1);
            var createdByIp = "127.0.0.1";
            
            return new RefreshToken(userId, token, expiresAt, createdByIp);
        }

        return new NoSpecimen();
    }
}
