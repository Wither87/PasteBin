using StackExchange.Redis;

namespace PasteBinASP.Cache.Impl;

public class HashCache : IHashCache
{
    private const string HashQueue = "HashQueue";
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _redis;

    public HashCache(IConnectionMultiplexer connection)
    {
        _connection = connection;
        _redis = _connection.GetDatabase();
    }

    public async Task FillAsync(params string[] values)
    {
        foreach (var item in values)
            await _redis.ListLeftPushAsync(HashQueue, item);
    }

    public async Task<string> GetAsync()
    {
        string? value = await _redis.ListRightPopAsync(HashQueue);
        if (value is null)
            throw new RedisException("Cache is empty");
        return value!;
    }

    public async Task<bool> IsEmptyAsync() => await _redis.ListLengthAsync(HashQueue) == 0;

    public async Task<bool> IsNotEmptyAsync() => !await IsEmptyAsync();
}
