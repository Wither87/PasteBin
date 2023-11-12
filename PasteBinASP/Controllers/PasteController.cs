using Microsoft.AspNetCore.Mvc;
using PasteBinASP.DataProviders;
using PasteBinASP.Extentions;
using PasteBinASP.Models;
using PasteBinASP.Services;

namespace PasteBinASP.Controllers;

/// <summary>
/// Контроллер для работы с текстами
/// </summary>
[Route("")]
public class PasteController : Controller
{
    private readonly ILogger<PasteController> _logger;
    private readonly IPasteService _pasteService;
    private readonly IDateTimeProvider _dateTimeOffsetProvider;

    public PasteController(
        ILogger<PasteController> logger,
        IPasteService pasteService,
        IDateTimeProvider dateTimeOffsetProvider)
    {
        _logger = logger;
        _pasteService = pasteService;
        _dateTimeOffsetProvider = dateTimeOffsetProvider;
    }

    /// <summary>
    /// Получить страницу с тектом по короткой ссылке
    /// </summary>
    /// <param name="shortLink">Короткая ссылка на текст</param>
    /// <returns></returns>
    [HttpGet("/{shortLink}")]
    public async Task<ActionResult> Index(string shortLink)
    {
        if (shortLink.IsNullOrEmpty())
            return RedirectToAction(nameof(Create));

        var paste = await _pasteService.GetAsync(shortLink);
        return View(paste);
    }

    /// <summary>
    /// Страница для добавления текста
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public ActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Обработка запроса на добавление текста
    /// </summary>
    /// <param name="model">Модель из формы</param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(PasteCreateModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var link = await _pasteService.CreateAsync(model.Text, model.LiveTimeInSeconds);
        return Redirect($"{link}");
    }
}
