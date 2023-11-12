using Microsoft.EntityFrameworkCore;
using PasteBinASP.DbContexts;
using PasteBinASP.Entities;

namespace PasteBinASP.Tests;

internal class PastebinDbContextFactory
{
    public static PastebinDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PastebinDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new PastebinDbContext(options);

        dbContext.ShortLinks.AddRange(new List<ShortLink>
        {
            new ShortLink()
            {
                Link = "356C57F6-32FE-4858-B6D4-A8CFBDF08B64",
                Created = new DateTimeOffset(2023,11,8,11,54,31,new TimeSpan()),
                DeleteDateTime = new DateTimeOffset(2023,11,8,12,54,31,new TimeSpan())
            },
            new ShortLink() 
            {
                Link = "2A44A50F-65AA-487B-88A0-0AD8DC684428",
                Created = new DateTimeOffset(2023,11,7,16,32,13,new TimeSpan()),
                DeleteDateTime = new DateTimeOffset(2023,11,8,16,32,13,new TimeSpan())
            },
            new ShortLink()
            {
                Link = "524E168E-AC16-4473-9797-FA30476D6C1E",
                Created = new DateTimeOffset(2023,11,8,19,13,24,new TimeSpan()),
                DeleteDateTime = new DateTimeOffset(2023,11,8,20,13,24,new TimeSpan())
            }
        });
        dbContext.SaveChanges();
        return dbContext;
    }

    public static void Destroy(PastebinDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }
}
