using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Common;

public abstract class TestBase : IDisposable
{
    protected readonly IFixture Fixture;
    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        Fixture = new Fixture();
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        Context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
