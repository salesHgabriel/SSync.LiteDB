using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Engine;
using SSync.Server.LitebDB.Enums;
using SSync.Server.LitebDB.Extensions;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Endpoints;
using SSync.Server.LiteDB.PlayGround.Model;
using SSync.Server.LiteDB.PlayGround.Sync;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSSyncSchemaCollection<TestDbContext>(
    optionsPullChanges: (pullChangesConfig) =>
    {
        pullChangesConfig
            .By<UserSync>("User")
            .ThenBy<FinanceSync>("Finance");
    },
optionsPushChanges: (pushChangesConfig) =>
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

// app.MapGet("/api/v1/sync/pull", async ([AsParameters] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection) =>
// {
//     var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter, new SSyncOptions()
//     {
//         Mode = Mode.DEBUG
//     });
//
//     return Results.Ok(pullChangesRemoter);
// });
//
// app.MapPost("/api/v1/sync/push", async (HttpContext httpContext, JsonArray changes, [FromServices] ISchemaCollection schemaCollection) =>
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

// app.MapGet("/api/v1/sync/pull-stream", ([AsParameters] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection) =>
// {
//     var pullChangesRemoter = schemaCollection.PullStreamChanges(parameter, new SSyncOptions()
//     {
//         Mode = Mode.DEBUG
//     });
//
//     return Results.Ok(pullChangesRemoter);
// });


app.AddCrudEndpoints();

app.Run();

async Task CreateDbIfNotExistAsync(IServiceProvider services, ILogger logger)
{
    await using var db = services.CreateScope().ServiceProvider.GetRequiredService<TestDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
}


public partial class Program{}