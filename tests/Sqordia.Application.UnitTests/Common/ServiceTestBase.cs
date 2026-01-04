using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Common;

public abstract class ServiceTestBase<TService> : IDisposable where TService : class
{
    protected readonly IFixture Fixture;
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IMapper> MapperMock;
    protected readonly Mock<IEmailService> EmailServiceMock;
    protected readonly Mock<ILogger<TService>> LoggerMock;
    protected readonly TService Sut;

    protected ServiceTestBase()
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

        // Setup common mocks
        MapperMock = new Mock<IMapper>();
        EmailServiceMock = new Mock<IEmailService>();
        LoggerMock = new Mock<ILogger<TService>>();

        // Create service under test
        Sut = CreateService();
    }

    protected abstract TService CreateService();

    protected async Task SeedDatabaseAsync()
    {
        await Context.SaveChangesAsync();
    }

    protected async Task<T> AddEntityAsync<T>(T entity) where T : class
    {
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    protected async Task<List<T>> AddEntitiesAsync<T>(IEnumerable<T> entities) where T : class
    {
        Context.Set<T>().AddRange(entities);
        await Context.SaveChangesAsync();
        return entities.ToList();
    }

    protected void SetupMapper<TSource, TDestination>(TSource source, TDestination destination)
    {
        MapperMock.Setup(x => x.Map<TDestination>(source)).Returns(destination);
    }

    protected void SetupMapper<TSource, TDestination>(IEnumerable<TSource> sources, IEnumerable<TDestination> destinations)
    {
        MapperMock.Setup(x => x.Map<IEnumerable<TDestination>>(sources)).Returns(destinations);
    }

    protected void SetupMapper<TSource, TDestination>(List<TSource> sources, List<TDestination> destinations)
    {
        MapperMock.Setup(x => x.Map<List<TDestination>>(sources)).Returns(destinations);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
