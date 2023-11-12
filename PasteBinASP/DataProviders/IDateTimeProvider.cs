namespace PasteBinASP.DataProviders;

public interface IDateTimeProvider
{
    DateTimeOffset DateTimeOffsetUtcNow { get; set; }
    DateTimeOffset DateTimeOffsetNow { get; set; }
    DateTime DateTimeUtcNow { get; set; }
    DateTime DateTimeNow { get; set; }
}

