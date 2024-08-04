using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;

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


app.MapGet("/list", async ([FromServices] TestDbContext cxt) =>
{

    var usersDb = await cxt.User.ToListAsync();

    return Results.Ok(usersDb);
});


app.MapGet("/create", async ([FromServices] TestDbContext cxt) =>
{

    var user = new User()
    {
        Id = Guid.NewGuid(),
        Name = $" Cotoso {DateTime.UtcNow.Ticks}",
    };
    cxt.User.Add(user);

    var finance = new Finance()
    {
        Id = Guid.NewGuid(),
        Price = new Random().Next(),
    };

    cxt.Finances.Add(finance);


    var res = await cxt.SaveChangesAsync();


    return Results.Ok(res);
});


app.MapGet("/pull", async ([AsParameters] PlayParamenter parameter, [FromServices] ISchemaCollection schemaCollection) =>
{


   var pullChangesRemoter = await  schemaCollection.PullChangesAsync(parameter);

    return Results.Ok(pullChangesRemoter);
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
}

class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

class Finance
{
    public Guid Id { get; set; }
    public double Price { get; set; }
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
        }).ToListAsync();

        return finances;
    }
}





