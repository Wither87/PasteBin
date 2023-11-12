using System.Net;

namespace PasteBinASP.ObjectEntities;

public abstract class ObjectEntityBase
{
    public HttpStatusCode StatusCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
