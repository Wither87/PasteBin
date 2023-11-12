using Microsoft.EntityFrameworkCore;
using PasteBinASP.Entities;

namespace PasteBinASP.DbContexts;

public class PastebinDbContext : DbContext
{
    const string ShortLinkSequenceName = "short_link_id_sequence";
    const string ShortLinkSequenceNextValueQueryString = $"select nextval('\"{ShortLinkSequenceName}\"') as \"Value\"";

    public PastebinDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<ShortLink> ShortLinks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence(ShortLinkSequenceName)
                    .IncrementsBy(1)
                    .HasMin(1)
                    .HasMax(int.MaxValue)
                    .StartsAt(1);

        modelBuilder.Entity<ShortLink>().HasKey(s => s.Link);
    }

    public async Task<int> GetNextSequenceValue() =>
        await Database.SqlQueryRaw<int>(ShortLinkSequenceNextValueQueryString).SingleAsync();

    public async Task<int[]> GetSeveralNextSequenceValue(int quantity)
    {
        var query = $"{ShortLinkSequenceNextValueQueryString} from generate_series(1,{quantity})";
        var result = await Database.SqlQueryRaw<int>(query).ToArrayAsync();
        return result;
    }
}
