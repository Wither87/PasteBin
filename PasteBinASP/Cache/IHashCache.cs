namespace PasteBinASP.Cache;

public interface IHashCache
{
    Task FillAsync(params string[] values);
    Task<string> GetAsync();
    Task<bool> IsEmptyAsync();
    Task<bool> IsNotEmptyAsync();
}
