using Moq;
using PasteBinASP.DataProviders;

namespace PasteBinASP.Tests;

internal class DateTimeProviderFactory
{
    public IDateTimeProvider Build()
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.DateTimeNow).Returns(DateTime.Now);
        dateTimeProviderMock.Setup(x => x.DateTimeUtcNow).Returns(DateTime.UtcNow);
        dateTimeProviderMock.Setup(x => x.DateTimeOffsetNow).Returns(DateTimeOffset.Now);
        dateTimeProviderMock.Setup(x => x.DateTimeOffsetUtcNow).Returns(DateTimeOffset.UtcNow);
        return dateTimeProviderMock.Object;
    }
}
