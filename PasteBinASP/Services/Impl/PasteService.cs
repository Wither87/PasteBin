using PasteBinASP.Cache;
using PasteBinASP.Constants;
using PasteBinASP.DataProviders;
using PasteBinASP.Entities;
using PasteBinASP.Extentions;
using PasteBinASP.Models;
using PasteBinASP.ObjectStorages;
using PasteBinASP.Repositories;
using System.Net;

namespace PasteBinASP.Services.Impl;

/// <summary>
/// Сервис для обрабоки текстов
/// </summary>
public class PasteService : IPasteService
{
    private const int CacheLiveTimeInDays = 1;

    private readonly ILogger<PasteService> _logger;
    private readonly IPasteRepository _pasteRepository;
    private readonly IPasteObjectStorage _pasteObjectStorage;
    private readonly IHashService _hashService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPasteCache _cache;

    #region Constructors
    public PasteService(
        ILogger<PasteService> logger,
        IPasteRepository pasteRepository,
        IPasteObjectStorage pasteObjectStorage,
        IHashService hashService,
        IDateTimeProvider dateTimeProvider,
        IPasteCache cache)
    {
        _logger = logger;
        _pasteRepository = pasteRepository;
        _pasteObjectStorage = pasteObjectStorage;
        _hashService = hashService;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
    }
    #endregion

    public async Task<string> CreateAsync(string text, int liveTimeInSeconds)
    {
        if (text.IsNullOrEmpty())
            return string.Empty;
        var deleteTimeInUtc = _dateTimeProvider.DateTimeOffsetUtcNow.AddSeconds(liveTimeInSeconds);
        if (deleteTimeInUtc < _dateTimeProvider.DateTimeOffsetUtcNow)
        {
            var exception = new ArgumentException($"Дата удаления {deleteTimeInUtc} не может быть раньше даты {_dateTimeProvider.DateTimeOffsetUtcNow}");
            _logger.LogError($"{exception.Message}. {exception.StackTrace}");
            throw exception;
        }

        var generatedLink = await _hashService.GetUniqueHashAsync();

        await _pasteObjectStorage.AddAsync(generatedLink, text);
        _logger.LogInformation($"Текст записан в файл [{generatedLink}]");
        var result = await _pasteRepository.AddAsync(generatedLink, deleteTimeInUtc);
        _logger.LogInformation($"Ссылка [{generatedLink}] добавлена в базу данных");

        return result.Link;
    }

    public async Task<PasteViewModel> GetAsync(string inputLink)
    {
        var cashedText = await _cache.GetAsync(inputLink);
        if (cashedText is not null)
        {
            _logger.LogInformation($"Текст по ссылке {inputLink} найден в кэше");
            _ = _pasteRepository.IncrementRequestsCountAsync(inputLink).ConfigureAwait(false);
            return new()
            {
                Text = cashedText,
                StatusCode = HttpStatusCode.OK
            };
        }
        // если в кэше нет, то идём в базу проверять активность
        if (await _pasteRepository.IsNotExtistsAsync(inputLink))
        {
            _logger.LogInformation($"Ссылка {inputLink} в базе данных не найдена");
            return new()
            {
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessage = ErrorMessages.NotFound
            };
        }

        var link = await _pasteRepository.GetByLinkAsync(inputLink);
        if (!link.IsActive)
        {
            _logger.LogInformation($"Ссылка {inputLink} больше не доступна");
            return new()
            {
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessage = ErrorMessages.NotAvailable
            };
        }
        var viewModel = (PasteViewModel)await _pasteObjectStorage.GetAsync(link.Link);
        _logger.LogInformation($"Текст по ссылке {inputLink} получен из хранилища");

        // Убрать хардкод. Добавить в конфиг
        // переделать логику на обращения в день, а не всего обращений как сейчас :\
        if (link.RequestsCount >= 5)
        {
            var cacheLiveTime = CalculateCacheLiveTime(link);
            await _cache.PutAsync(link.Link, viewModel.Text, cacheLiveTime);
        }

        return viewModel;
    }

    private TimeSpan CalculateCacheLiveTime(ShortLink link)
    {
        var tempTimeSpan = _dateTimeProvider.DateTimeOffsetUtcNow - link.DeleteDateTime;
        // если время жизни текста больше 24 часов - добавить в кэш на 24 часа
        // иначе добавить на оставшееся время жизни
        return tempTimeSpan.Days > CacheLiveTimeInDays 
            ? TimeSpan.FromDays(CacheLiveTimeInDays) 
            : tempTimeSpan;
    }
}
