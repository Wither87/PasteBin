namespace PasteBinASP.Models;

public class PasteCreateModel
{
    public string Text { get; set; } = string.Empty;
    public int LiveTimeInSeconds { get; set; }
}
