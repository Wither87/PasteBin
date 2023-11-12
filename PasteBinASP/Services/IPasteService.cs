using PasteBinASP.Models;

namespace PasteBinASP.Services;

public interface IPasteService
{
    Task<string> CreateAsync(string text, int liveTimeInSeconds);
    Task<PasteViewModel> GetAsync(string inputLink);
}
