using PasteBinASP.ObjectEntities;
using System.Net;

namespace PasteBinASP.Models;

public class PasteViewModel
{
    public string Text { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;


    public static explicit operator PasteViewModel(PasteObjectEntity paste) =>
        new() { Text = paste.Text, StatusCode = paste.StatusCode, ErrorMessage = paste.ErrorMessage };
}
