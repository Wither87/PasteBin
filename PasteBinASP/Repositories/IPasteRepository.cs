using PasteBinASP.Entities;

namespace PasteBinASP.Repositories;

public interface IPasteRepository
{
    Task<ShortLink> AddAsync(string link, DateTimeOffset deleteTime);
    Task<ShortLink> GetByLinkAsync(string inputLink);
    Task<ShortLink> IncrementRequestsCountAsync(string inputLink);
    Task<int> GetUniqueNumberAsync();
    Task<int[]> GetUniqueNumbersAsync(int quantity);
    Task<bool> IsExtistsAsync(string link);
    Task<bool> IsNotExtistsAsync(string link);
    Task<IQueryable<ShortLink>> DeleteExpiredLinksAsync();
}
