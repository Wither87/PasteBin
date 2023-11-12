using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using PasteBinASP.DataProviders;
using PasteBinASP.DbContexts;
using PasteBinASP.Entities;
using PasteBinASP.Repositories.Impl;

namespace PasteBinASP.Tests.Repositories;

internal class PasteRepositoryTests
{
    private readonly Fixture _fixture = new();
    private ILogger<PasteRepository> _logger;
    private PastebinDbContext _dbContext;
    private IDateTimeProvider _dateTimeProvider;
    private PasteRepository _repository;
    private string _link;

    [SetUp]
    public void SetUp()
    {
        _logger = Mock.Of<ILogger<PasteRepository>>();
        _dateTimeProvider = new DateTimeProviderFactory().Build();

        _dbContext = PastebinDbContextFactory.Create();

        _repository = new PasteRepository(_logger, _dbContext, _dateTimeProvider);
        _link = _fixture.Create<string>();
    }

    [TearDown]
    public void TearDown()
    {
        PastebinDbContextFactory.Destroy(_dbContext);
    }

    [Test]
    public async Task AddAsync_ReturnsShortLink()
    {
        var created = _dateTimeProvider.DateTimeOffsetUtcNow;
        var deleteDateTime = created.AddDays(1);
        var expected = new ShortLink
        {
            Link = _link,
            Created = created,
            DeleteDateTime = deleteDateTime,
        };

        var actual = await _repository.AddAsync(_link, deleteDateTime);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetByLinkAsync_ReturnsShortLink()
    {
        var link = "2A44A50F-65AA-487B-88A0-0AD8DC684428";
        var expected = new ShortLink()
        {
            Link = "2A44A50F-65AA-487B-88A0-0AD8DC684428",
            Created = new DateTimeOffset(2023, 11, 7, 16, 32, 13, new TimeSpan()),
            DeleteDateTime = new DateTimeOffset(2023, 11, 8, 16, 32, 13, new TimeSpan()),
            RequestsCount = 1
        };

        var actual = await _repository.GetByLinkAsync(link);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task IsExtistsAsync_ReturnsTrue()
    {
        var link = "2A44A50F-65AA-487B-88A0-0AD8DC684428";
        var actual = await _repository.IsExtistsAsync(link);
        Assert.That(actual, Is.True);
    }

    [Test]
    public async Task IsExtistsAsync_ReturnsFalse()
    {
        var link = _fixture.Create<string>();
        var actual = await _repository.IsExtistsAsync(link);
        Assert.That(actual, Is.False);
    }

    [Test]
    public async Task IsNotExtistsAsync_ReturnsTrue()
    {
        var link = _fixture.Create<string>();
        var actual = await _repository.IsNotExtistsAsync(link);
        Assert.That(actual, Is.True);
    }

    [Test]
    public async Task IsNotExtistsAsync_ReturnsFalse()
    {
        var link = "2A44A50F-65AA-487B-88A0-0AD8DC684428";
        var actual = await _repository.IsNotExtistsAsync(link);
        Assert.That(actual, Is.False);
    }
}
