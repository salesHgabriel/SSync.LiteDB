using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LiteDB.PlayGround.Model;

namespace SSync.Server.LiteDB.PlayGround.Data;

public class TestDbContext : DbContext, ISSyncDbContextTransaction
{
    private IDbContextTransaction? transaction;
    private readonly IConfiguration _configuration;


    public TestDbContext(DbContextOptions<TestDbContext> options, IConfiguration configuration)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        
        _configuration = configuration;
    }

    public DbSet<User> User => Set<User>();
    public DbSet<Finance> Finances => Set<Finance>();

    public async Task BeginTransactionSyncAsync()
        => transaction = await Database.BeginTransactionAsync();

    public async Task CommitSyncAsync()
        => await Database.CommitTransactionAsync();

    public Task CommitTransactionSyncAsync()
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.CommitAsync();
    }

    public Task RollbackTransactionSyncAsync()
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return transaction.RollbackAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Id).HasConversion(new GuidToStringConverter());

        modelBuilder.Entity<Finance>()
            .Property(u => u.Id).HasConversion(new GuidToStringConverter());

        base.OnModelCreating(modelBuilder);
    }
    
}