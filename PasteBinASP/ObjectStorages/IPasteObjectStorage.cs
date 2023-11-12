using PasteBinASP.ObjectEntities;

namespace PasteBinASP.ObjectStorages;

public interface IPasteObjectStorage
{
    Task AddAsync(string key, string value);
    Task<PasteObjectEntity> GetAsync(string key);
    Task DeleteAsync(params string[] keys);

}
