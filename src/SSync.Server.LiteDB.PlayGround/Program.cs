using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Engine;
using SSync.Server.LitebDB.Enums;
using SSync.Server.LitebDB.Extensions;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Model;
using SSync.Server.LiteDB.PlayGround.Sync;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

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


builder.Services.AddDbContext<TestDbContext>(optionsBuilder => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Pooling=true;Database=poc_ssync;User Id=postgres;Password=postgres;"));

var app = builder.Build();

await CreateDbIfNotExistAsync(app.Services, app.Logger);

app.UseHttpsRedirection();

app.MapApiEndpointsSync<PlayParamenter>();


//custom endpoints

// app.MapGet("/pull", async ([AsParameters] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection) =>
// {
//     var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter, new SSyncOptions()
//     {
//         Mode = Mode.DEBUG
//     });
//
//     return Results.Ok(pullChangesRemoter);
// });
//
// app.MapPost("/push", async (HttpContext httpContext, JsonArray changes, [FromServices] ISchemaCollection schemaCollection) =>
// {
//     var query = httpContext.Request.Query;
//
//     var parameter = new SSyncParameter
//     {
//         // Time = Convert.ToInt32(query["time"]),
//         Colletions = query["colletions"].ToArray()!,
//         Timestamp = DateTime.TryParse(query["timestamp"], out DateTime timestamp) ? timestamp : DateTime.MinValue
//     };
//
//     var isOk = await schemaCollection.PushChangesAsync(changes, parameter, new SSyncOptions()
//     {
//         Mode = Mode.DEBUG
//     });
//
//     return Results.Ok(isOk);
// });

// app.MapGet("/pull-stream", ([AsParameters] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection) =>
// {
//     var pullChangesRemoter = schemaCollection.PullStreamChanges(parameter, new SSyncOptions()
//     {
//         Mode = Mode.DEBUG
//     });
//
//     return Results.Ok(pullChangesRemoter);
// });


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

    var user = cxt.User.Find(userId);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.Name = $"Update {new Random().Next()}";

    user.SetUpdatedAt(DateTime.UtcNow);

    cxt.User.Update(user);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/finance-update/{id}", async (Guid id, [FromServices] TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var finance = cxt.Finances.Find(id);

    if (finance is null)
    {
        return Results.NotFound();
    }

    finance.Price = new Random().Next();

    finance.SetUpdatedAt(DateTime.UtcNow);

    cxt.Finances.Update(finance);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/finance-delete/{id}", async (Guid id, TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var finance = cxt.Finances.Find(id);

    if (finance is null)
    {
        return Results.NotFound();
    }

    finance.SetDeletedAt(DateTime.UtcNow);

    cxt.Finances.Update(finance);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.MapGet("/user-delete/{id}", async (Guid id, TestDbContext cxt) =>
{
    var now = DateTime.UtcNow;

    var user = cxt.User.Find(id);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.SetDeletedAt(DateTime.UtcNow);

    cxt.User.Update(user);

    var res = await cxt.SaveChangesAsync();

    return Results.Ok(res);
});

app.Run();

async Task CreateDbIfNotExistAsync(IServiceProvider services, ILogger logger)
{
    await using var db = services.CreateScope().ServiceProvider.GetRequiredService<TestDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
}


public partial class Program{}