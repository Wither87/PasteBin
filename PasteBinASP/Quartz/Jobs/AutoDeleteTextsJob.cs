using PasteBinASP.ObjectStorages;
using PasteBinASP.Repositories;
using Quartz;

namespace PasteBinASP.Quartz.Jobs;

public class AutoDeleteTextsJob : IJob
{
    private readonly IPasteRepository _pasteRepository;
    private readonly IPasteObjectStorage _pasteObjectStorage;

    public AutoDeleteTextsJob(IPasteRepository pasteRepository, IPasteObjectStorage pasteObjectStorage)
    {
        _pasteRepository = pasteRepository;
        _pasteObjectStorage = pasteObjectStorage;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        var links = await _pasteRepository.DeleteExpiredLinksAsync();
        await _pasteObjectStorage.DeleteAsync(links.Select(x => x.Link).ToArray());
    }
}
