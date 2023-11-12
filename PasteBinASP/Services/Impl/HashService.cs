using PasteBinASP.Cache;
using PasteBinASP.Repositories;
using System.Text;

namespace PasteBinASP.Services.Impl;

public class HashService : IHashService
{
    private const int CashSize = 100;

    private readonly ILogger<HashService> _logger;
    private readonly IPasteRepository _pasteRepository;
    private readonly IHashCache _cache;

    public HashService(ILogger<HashService> logger,
        IPasteRepository repository,
        IHashCache cache)
    {
        _logger = logger;
        _pasteRepository = repository;
        _cache = cache;
    }

    public string CalculateHash(string input)
    {
        var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        result = result.Replace("=", "");
        return result;
    }

    public string CalculateHash(int input) => CalculateHash(input.ToString());

    public async Task<string> GetUniqueHashAsync()
    {
        if (await _cache.IsNotEmptyAsync())
            return await _cache.GetAsync();

        _logger.LogInformation($"Кэш уникальных значений пустой.");

        var hashes = await GenerateHashesAsync();
        var returnHash = hashes[0];

        await _cache.FillAsync(hashes[1..]);

        return returnHash;
    }

    private async Task<string[]> GenerateHashesAsync()
    {
        var hashes = new List<string>();
        var uniqueNumbers = await _pasteRepository.GetUniqueNumbersAsync(CashSize);
        _logger.LogInformation($"Получено {CashSize} уникальных значений из базы данных. Значения: {string.Join(' ', uniqueNumbers)}.");
        foreach (var number in uniqueNumbers)
        {
            var hash = CalculateHash(number);
            hashes.Add(hash);
        }
        return hashes.ToArray();
    }

    private async Task<string> GenerateHashAsync()
    {
        var uniqeNum = await _pasteRepository.GetUniqueNumberAsync();
        _logger.LogInformation($"Получено одно уникальное значение из базы данных. Значение: {uniqeNum}.");
        return CalculateHash(uniqeNum);
    }
}
