using StackExchange.Redis;

namespace PasteBinASP.Cache.Impl;

public class PasteCache : IPasteCache
{
    private readonly ILogger<PasteCache> _logger;
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _redis;
    public PasteCache(ILogger<PasteCache> logger, IConnectionMultiplexer connection)
    {
        _logger = logger;
        _connection = connection;
        _redis = _connection.GetDatabase();
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _redis.StringGetAsync(key);
        var logMessage = value == RedisValue.Null
            ? $"Текст по ключу {key} не найден."
            : $"Текст по ключу {key} найден. Текст: {value}.";
        _logger.LogInformation(logMessage);
        return value;
    }

    public async Task PutAsync(string key, string value, TimeSpan liveTime)
    {
        await _redis.StringSetAsync(key, value, liveTime);
        _logger.LogInformation($"Текст по ключу {value} добавлен в кэш");
    }
}
