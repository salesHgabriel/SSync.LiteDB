using SSync.Server.LitebDB.Engine;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSSyncSchemaCollection();

var app = builder.Build();
app.UseHttpsRedirection();


app.MapGet("/test", () =>
{
    Results.Ok(1);
});

app.Run();

