using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;
using SSync.Shared.ClientServer.LitebDB.Enums;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSSyncSchemaCollection<TestDbContext>(
    (pullChangesConfig) =>
{
    pullChangesConfig
       .By<UserSync>("User")
       .ThenBy<FinanceSync>("Finance");
},
(pushChangesConfig) =>
{
    pushChangesConfig
       .By<UserSync>("User")
       .ThenBy<FinanceSync>("Finance");
});

var connectionString = builder.Configuration.GetConnectionString("SqliteConnectionString")
     ?? "Data Source=Examples\\MyRemote.db";

builder.Services.AddSqlite<TestDbContext>(connectionString);

builder.Services.AddScoped(_ => new SqliteConnection(connectionString));

var app = builder.Build();

await CreateDBIfNotExistAsync(app.Services, app.Logger);

app.UseHttpsRedirection();

app.MapGet("/pull", async ([AsParameters] PlayParamenter parameter, [FromServices] ISchemaCollection schemaCollection) =>
{
    var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter, new SSyncOptions()
    {
        Mode = Mode.DEBUG
    });

    return Results.Ok(pullChangesRemoter);
});

app.MapPost("/push", async (HttpContext httpContext, JsonArray changes, [FromServices] ISchemaCollection schemaCollection) =>
{
    var query = httpContext.Request.Query;

    var parameter = new PlayParamenter
    {
        Time = Convert.ToInt32(query["time"]),
        Colletions = query["colletions"].ToArray()!,
        Timestamp = long.TryParse(query["timestamp"], out long timestamp) ? timestamp : 0
    };

    var isOk = await schemaCollection.PushChangesAsync(changes, parameter, new SSyncOptions()
    {
        Mode = Mode.DEBUG
    });

    return Results.Ok(isOk);
});

app.MapGet("/list", async ([FromServices] TestDbContext cxt) =>
{
    var usersDb = await cxt.User.ToListAsync();
    var financesDb = await cxt.Finances.ToListAsync();

    return Results.Ok(new { usersDb, financesDb });
});

app.MapGet("/create", async ([FromServices] TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;
    var user = new User()
    {
        Id = Guid.NewGuid(),
        Name = $" Cotoso {new Random().Next()}",
        CreatedAt = now,
        UpdatedAt = now
    };
    cxt.User.Add(user);

    var finance = new Finance()
    {
        Id = Guid.NewGuid(),
        Price = new Random().Next(),
        CreatedAt = now,
        UpdatedAt = now
    };

    cxt.Finances.Add(finance);

    var res = await cxt.SaveChangesAsync();
    return Results.Ok(res);
});

app.MapGet("/user-update/{userId}", async (Guid userId, [FromServices] TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var user = cxt.User.SingleOrDefault(u => u.Id == userId);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.Name = $"Update {new Random().Next()}";

    user.UpdatedAt = DateTime.UtcNow;

    cxt.User.Update(user);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/finance-update/{id}", async (Guid id, [FromServices] TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var finance = cxt.Finances.SingleOrDefault(u => u.Id == id);

    if (finance is null)
    {
        return Results.NotFound();
    }

    finance.Price = new Random().Next();

    finance.UpdatedAt = DateTime.UtcNow;

    cxt.Finances.Update(finance);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/finance-delete/{id}", async (Guid id, TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var finance = cxt.Finances.SingleOrDefault(f => f.Id == id);

    if (finance is null)
    {
        return Results.NotFound();
    }

    finance.DeletedAt = DateTime.UtcNow;

    cxt.Finances.Update(finance);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/user-delete/{id}", async (Guid id, TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var user = cxt.User.SingleOrDefault(u => u.Id == id);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.DeletedAt = DateTime.UtcNow;

    cxt.User.Update(user);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.Run();

async Task CreateDBIfNotExistAsync(IServiceProvider services, ILogger logger)
{
    using var db = services.CreateScope().ServiceProvider.GetRequiredService<TestDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
}

internal class TestDbContext : DbContext, ISSyncDbContextTransaction
{
    private IDbContextTransaction? transaction;

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options) { }

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

internal class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

internal class Finance
{
    public Guid Id { get; set; }
    public double Price { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

internal class PlayParamenter : SSyncParamenter
{
    public int Time { get; set; } = new Random().Next(100);
}

internal class UserSync : ISchema
{
    public UserSync(Guid id) : base(id)
    {
    }

    public string? Name { get; set; }
}

internal class FinanceSync : ISchema
{
    public FinanceSync(Guid id) : base(id)
    {
    }

    public double Price { get; set; }
}

internal class PullUserRequesHandler : ISSyncPullRequest<UserSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullUserRequesHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<UserSync>> QueryAsync(PlayParamenter parameter)
    {
        var users = await _ctx.User.Select(u => new UserSync(u.Id)
        {
            Name = u.Name,
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return users;
    }
}

internal class PullFinanceRequesHandler : ISSyncPullRequest<FinanceSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullFinanceRequesHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<FinanceSync>> QueryAsync(PlayParamenter parameter)
    {
        var finances = await _ctx.Finances.Select(u => new FinanceSync(u.Id)
        {
            Price = new Random().Next(100),
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return finances;
    }
}

internal class PushUserRequestHandler : ISSyncPushRequest<UserSync>
{
    private readonly TestDbContext _db;

    public PushUserRequestHandler(TestDbContext db) => _db = db;

    public async Task<UserSync?> FindByIdAsync(Guid id)
    {
        return await _db.User.Where(u => u.Id == id)
            .Select(us => new UserSync(id)
            {
                Name = us.Name,
                CreatedAt = us.CreatedAt,
                DeletedAt = us.DeletedAt,
                UpdatedAt = us.UpdatedAt
            })
        .FirstOrDefaultAsync();
    }

    public async Task<bool> CreateAsync(UserSync schema)
    {
        var us = new User()
        {
            Id = schema.Id,
            Name = schema.Name,
            CreatedAt = schema.CreatedAt,
            DeletedAt = schema.DeletedAt,
            UpdatedAt = schema.UpdatedAt
        };

        await _db.User.AddAsync(us);

        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(UserSync schema)
    {
        var us = await _db.User.FindAsync(schema.Id);

        us.UpdatedAt = DateTime.Now;

        us.Name = schema.Name;

        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(UserSync schema)
    {
        var us = await _db.User.FindAsync(schema.Id);

        us.DeletedAt = DateTime.Now;

        return await _db.SaveChangesAsync() > 0;
    }
}