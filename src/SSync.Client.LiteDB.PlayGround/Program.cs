using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Sync;

try
{
    var colName = nameof(User);
    var colNameEstoque = nameof(Estoque);

    string dirApp = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()).ToString() + "\\Examples\\"; // Only test, NOT PRD !!

    var path = dirApp + "MyData.db";

    using var db = new LiteDatabase(path);

    var sync = new Synchronize(db, new SynchronizeOptions()
    {
        PathFile = dirApp,
        Mode = Mode.DEBUG,
        SaveLogOnFile = true
    });

    //var colUser = db.GetCollection<User>(colName);

    //var user = new User(Guid.NewGuid())
    //{
    //    Name = $"Gabriel Sales {DateTime.UtcNow.Ticks}"
    //};

    //user.CreateAt();
    //colUser.Insert(user);

    //var user = colUser.FindOne(u => u.Id == Guid.Parse("8ade4fa5-3077-411d-a6a9-782c0bde278e"));

    //user.Name = "Gabriel atualizado 3";

    //user.UpdateAt();

    //user.DeleteAt();

    //colUser.Update(user);

    //var colEstoque = db.GetCollection<Estoque>(colNameEstoque);

    //var estoque = new Estoque(Guid.NewGuid())
    //{
    //    Valor = new Random().Next(50)
    //};

    //estoque.CreateAt();
    //colEstoque.Insert(estoque);

    //pull local

    var x = 0;

    var now = DateTime.UtcNow;

    var pullChangesBuilder = new SyncPullBuilder();

    pullChangesBuilder
        .AddPullSync(() => sync.PullChangesResultAsync<User>(x, colName, now))
        .AddPullSync(() => sync.PullChangesResultAsync<Estoque>(x, colNameEstoque, now))
        .Build();

    var databaseLocal = pullChangesBuilder.DatabaseLocalChanges;

    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(databaseLocal, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));

    //push because it is being used by another process.)'

   // string responseServer = await File.ReadAllTextAsync(dirApp + "ResponseServer.json");

    //var pushBuilder = new SyncPushBuilder(responseServer);

    //pushBuilder
    //    .AddPushSchemaSync<User>(change => sync.PushChangesResult(change), colName)
    //    .AddPushSchemaSync<Estoque>(change => sync.PushChangesResult(change), colNameEstoque)
    //    .Build();

}
catch (Exception ex)
{
    Console.WriteLine("ERROR: " + ex.Message);
}

Console.WriteLine("\n\nEnd.");
Console.ReadKey();

public class User : BaseSync
{
    public User(Guid id) : base(id)
    {
    }

    public string Name { get; set; } = string.Empty;
}

public class Estoque : BaseSync
{
    public Estoque(Guid id) : base(id)
    {
    }

    public int Valor { get; set; }
}