using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PasteBinASP.Controllers;
using PasteBinASP.DataProviders;
using PasteBinASP.Models;
using PasteBinASP.Services;

namespace PasteBinASP.Tests.Controllers;

internal class PasteControllerTests
{
    private Fixture _fixture = new();
    private ILogger<PasteController> _logger;
    private IPasteService _pasteService;
    private IDateTimeProvider _dateTimeOffsetProvider;
    private PasteController _controller;

    private int _liveTimeInSeconds;
    private string _text;

    [SetUp]
    public void Setup()
    {
        _logger = Mock.Of<ILogger<PasteController>>();
        _liveTimeInSeconds = _fixture.Create<int>();
        _text = _fixture.Create<string>();
        var pasteServiceMock = new Mock<IPasteService>();
        pasteServiceMock.Setup(x=>x.Create(_text, _liveTimeInSeconds)).Returns(Task.FromResult(_fixture.Create<string>()));
        _pasteService = pasteServiceMock.Object;
        _dateTimeOffsetProvider = Mock.Of<IDateTimeProvider>();

        _controller = new PasteController(_logger, _pasteService, _dateTimeOffsetProvider);
    }

    [Test]
    public async Task Index_NullValue_ReturnsRedirectToActionResult()
    {
        var actual = await _controller.Index(null);
        Assert.That(actual, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public async Task Index_EmptyValue_ReturnsRedirectToActionResult()
    {
        var actual = await _controller.Index("");
        Assert.That(actual, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public async Task Index_NotEmptyValue_ReturnsViewResult()
    {
        var value = _fixture.Create<string>();
        var actual = await _controller.Index(value);
        Assert.That(actual, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void HttpGetCreate_ReturnsViewResult()
    {
        var actual = _controller.Create();
        Assert.That(actual, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task HttpPostCreate_ModelIsValid_EqualsTrue_ReturnsRedirectResult()
    {
        var model = new PasteCreateModel
        {
            Text = _text,
            LiveTimeInSeconds = _liveTimeInSeconds
        };
        var actual = await _controller.Create(model);
        Assert.That(actual, Is.InstanceOf<RedirectResult>());
    }

    [Test]
    [Ignore("Без реализации")]
    public void HttpPostCreate_ModelIsValid_EqualsFalse_ReturnsRedirectResult()
    {
        // TODO Надо как-то подменить ModelState, либо задать правила валидации для модели
        //var model = new PasteCreateModel();
        //var actual = _controller.Create(model);
        //Assert.That(actual, Is.InstanceOf<ViewResult>());
        Assert.Pass();
    }
}
