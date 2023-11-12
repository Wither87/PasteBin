using Microsoft.EntityFrameworkCore;
using PasteBinASP.DataProviders;
using PasteBinASP.DbContexts;
using PasteBinASP.Entities;

namespace PasteBinASP.Repositories.Impl;

/// <summary>
/// Репозиторий для работы с текстами
/// </summary>
public class PasteRepository : IPasteRepository
{
    private readonly ILogger<PasteRepository> _logger;
    private readonly PastebinDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public PasteRepository(
        ILogger<PasteRepository> logger, 
        PastebinDbContext dbContext, 
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Добавляет новую ссылку в базу
    /// </summary>
    /// <param name="link">Короткая ссылка на текст</param>
    /// <returns>Короткая ссылка на текст</returns>
    public async Task<ShortLink> AddAsync(string link, DateTimeOffset deleteTime)
    {
        var shortLink = new ShortLink
        {
            Link = link,
            Created = _dateTimeProvider.DateTimeOffsetUtcNow,
            DeleteDateTime = deleteTime,
        };
        await _dbContext.AddAsync(shortLink);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"Add {link} into ShortLink table.");
        return shortLink;
    }

    /// <summary>
    /// Выдаёт короткую ссылку на текст
    /// </summary>
    /// <param name="inputLink">короткая ссылка</param>
    /// <returns>Короткая ссылка на текст</returns>
    public async Task<ShortLink> GetByLinkAsync(string inputLink)
    {
        var link = await _GetByLinkAsync(inputLink);
        link = await _IncrementRequestsCountAsync(link);
        return link;
    }

    private async Task<ShortLink> _GetByLinkAsync(string inputLink)
        => await _dbContext.ShortLinks.SingleAsync(x => x.Link == inputLink);


    public async Task<ShortLink> IncrementRequestsCountAsync(string inputLink)
        => await GetByLinkAsync(inputLink);

    private async Task<ShortLink> _IncrementRequestsCountAsync(ShortLink link)
    {
        link.RequestsCount++;
        _dbContext.ShortLinks.Update(link);
        await _dbContext.SaveChangesAsync();
        return link;
    }

    /// <summary>
    /// Выдаёт уникальное значение из последовательности в базе данных
    /// </summary>
    /// <returns>Значение последовательности</returns>
    public async Task<int> GetUniqueNumberAsync()
    {
        var nextId = await _dbContext.GetNextSequenceValue();
        _logger.LogInformation($"Return next id: {nextId} from ShortLink table.");
        return nextId;
    }

    public async Task<int[]> GetUniqueNumbersAsync(int quantity)
    {
        var nextId = await _dbContext.GetSeveralNextSequenceValue(quantity);
        _logger.LogInformation($"Return next id: {nextId} from ShortLink table.");
        return nextId;
    }

    public async Task<bool> IsExtistsAsync(string link) =>
        await _dbContext.ShortLinks.AnyAsync(o => o.Link == link);

    public async Task<bool> IsNotExtistsAsync(string link) =>
        !await IsExtistsAsync(link);

    public async Task<IQueryable<ShortLink>> DeleteExpiredLinksAsync()
    {
        var currentDateTime = _dateTimeProvider.DateTimeOffsetUtcNow;
        var links = _dbContext.ShortLinks.Where(x => currentDateTime > x.DeleteDateTime && x.IsActive);
        await links.ForEachAsync(x => x.IsActive = false);
        await _dbContext.SaveChangesAsync();

        return links;
    }
}
