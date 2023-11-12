namespace PasteBinASP.DataProviders.Impl;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset DateTimeOffsetUtcNow { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DateTimeOffsetNow { get; set; } = DateTimeOffset.Now;
    public DateTime DateTimeUtcNow { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeNow { get; set; } = DateTime.Now;
}

