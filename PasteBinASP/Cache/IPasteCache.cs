namespace PasteBinASP.Cache;

public interface IPasteCache
{
    Task<string?> GetAsync(string key);
    Task PutAsync(string key, string value, TimeSpan liveTime);
}
