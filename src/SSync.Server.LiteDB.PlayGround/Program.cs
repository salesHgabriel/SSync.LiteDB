using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;
using SSync.Shared.ClientServer.LitebDB.Extensions;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSSyncSchemaCollection((pullChangesConfig) => 
{
     pullChangesConfig
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


    var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter);

    return Results.Ok(pullChangesRemoter);
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
        Name = $" Cotoso {DateTime.UtcNow.ToUnixTimestamp()}",
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

app.MapGet("/update/{userId}", async ( Guid userId, [FromServices] TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var user = cxt.User.SingleOrDefault(u => u.Id == userId);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.Name = $"Update {DateTime.UtcNow.ToUnixTimestamp()}";
    
    user.UpdatedAt = DateTime.UtcNow;

    cxt.User.Update(user);

    var res = await cxt.SaveChangesAsync();



    return Results.Ok(res);
});

app.MapGet("/delete/{userId}", async (Guid userId ,TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;
   
    var user =  cxt.User.SingleOrDefault(u => u.Id == userId);

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

class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options) { }
    public DbSet<User> User => Set<User>();
    public DbSet<Finance> Finances => Set<Finance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>()
            .Property(u => u.Id).HasConversion(new GuidToStringConverter());

        modelBuilder.Entity<Finance>()
            .Property(u => u.Id).HasConversion(new GuidToStringConverter());

        base.OnModelCreating(modelBuilder);
    }
}

class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

class Finance
{
    public Guid Id { get; set; }
    public double Price { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}




class PlayParamenter : SSyncParamenter
{
    public int Time { get; set; } = new Random().Next(100);
}


class UserSync : ISchema
{
    public UserSync(Guid id) : base(id)
    {
    }

    public string? Name { get; set; }
}



class FinanceSync : ISchema
{
    public FinanceSync(Guid id) : base(id)
    {
    }

    public double Price { get; set; }
}


class PullUserRequesHandler : ISSyncPullRequest<UserSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullUserRequesHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<UserSync>> Query(PlayParamenter parameter)
    {

        var users =  await _ctx.User.Select(u => new UserSync(u.Id)
        {
            Name = u.Name,
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return users;
    }
}


class PullFinanceRequesHandler : ISSyncPullRequest<FinanceSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullFinanceRequesHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<FinanceSync>> Query(PlayParamenter parameter)
    {
        var finances = await _ctx.User.Select(u => new FinanceSync(u.Id)
        {
            Price = new Random().Next(100),
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return finances;
    }
}





