using Bogus;
using PasteBinASP.DbContexts;
using PasteBinASP.Entities;

namespace PasteBinASP.DataSeeders;

internal class ShortLinkDataSeeder
{
    public void Seed(PastebinDbContext dbContext)
    {
        if (dbContext is null || dbContext.ShortLinks.Any())
            return;

        var faker = new Faker<ShortLink>()
            .RuleFor(x => x.Link, x => x.Random.String2(10))
            .RuleFor(x => x.Created, x => x.Date.PastOffset(1, DateTimeOffset.UtcNow))
            .RuleFor(x => x.DeleteDateTime, x => x.Date.FutureOffset())
            .RuleFor(x => x.RequestsCount, x => x.Random.Int(1, 100))
            .RuleFor(x => x.IsActive, x => x.Random.Bool());

        var fakeData = faker.Generate(100);
        
        dbContext.ShortLinks.AddRange(fakeData);
        dbContext.SaveChanges();
    }
}
