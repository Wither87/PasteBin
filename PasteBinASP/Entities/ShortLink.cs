namespace PasteBinASP.Entities;

public class ShortLink
{
    public required string Link { get; set; }
    public required DateTimeOffset Created { get; set; }
    public required DateTimeOffset DeleteDateTime { get; set; }
    public int RequestsCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public override bool Equals(object? obj)
    {
        return obj is ShortLink link &&
               Link == link.Link &&
               Created.Equals(link.Created) &&
               DeleteDateTime.Equals(link.DeleteDateTime) &&
               RequestsCount == link.RequestsCount &&
               IsActive == link.IsActive;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Link, Created, DeleteDateTime, RequestsCount, IsActive);
    }
}
