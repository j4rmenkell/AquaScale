using Microsoft.EntityFrameworkCore;
using Xunit;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Models.Mirror;
using Microsoft.Extensions.Configuration;

namespace AquaScale.Api.Tests.Integration;

public class MirrorJoinTests : IAsyncLifetime
{
    private AquaScaleDbContext _db = null!;
    private Guid _testMeterId;
    private Guid _mirrorAccountMeterId;

    public async Task InitializeAsync()
    {   
         var config = new ConfigurationBuilder()
            .AddUserSecrets<MirrorJoinTests>()
            .Build();

        var connectionString = config.GetConnectionString("DevTest")
            ?? throw new InvalidOperationException("DevTest connection string not set — run dotnet user-secrets set");

        var options = new DbContextOptionsBuilder<AquaScaleDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        _db = new AquaScaleDbContext(options);

        // Use a REAL row Syncer already wrote — don't fabricate a GUID.
        var existingId = await _db.Set<MirrorAccountMeter>()
            .Select(m => m.Id)
            .FirstOrDefaultAsync();

        Assert.NotEqual(Guid.Empty, existingId); // fails loudly if DevTest has no synced data

        _mirrorAccountMeterId = existingId;
        _testMeterId = Guid.NewGuid();

        _db.Set<Meter>().Add(new Meter { Id = _testMeterId, MirrorAcctmtrId = _mirrorAccountMeterId });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        var probe = await _db.Set<Meter>().FindAsync(_testMeterId);
        if (probe != null)
        {
            _db.Set<Meter>().Remove(probe); // meters is Api-owned — safe to delete
            await _db.SaveChangesAsync();
        }
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task RawLinqJoin_ResolvesAcrossOwnershipBoundary()
    {
        var result = await (
            from meter in _db.Set<Meter>()
            join mam in _db.Set<MirrorAccountMeter>() on meter.MirrorAcctmtrId equals mam.Id
            where meter.Id == _testMeterId
            select new { meter.Id, mam.MeterNo, mam.MeterStatus }
        ).FirstOrDefaultAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Include_NavigationProperty_ResolvesAcrossOwnershipBoundary()
    {
        var meter = await _db.Set<Meter>()
            .Include(m => m.MirrorAccountMeter)
            .FirstOrDefaultAsync(m => m.Id == _testMeterId);

        Assert.NotNull(meter);
        Assert.NotNull(meter!.MirrorAccountMeter); // the one more likely to surprise you
        Assert.Equal(_mirrorAccountMeterId, meter.MirrorAccountMeter!.Id);
    }
}