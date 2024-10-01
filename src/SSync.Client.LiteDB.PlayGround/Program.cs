using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Enums;
using SSync.Client.LitebDB.Sync;

try
{
    var colName = nameof(User);
    var colNameEstoque = nameof(Estoque);

    string dirApp = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory)!.ToString())!.ToString())!.ToString()! + "\\Examples\\"; // Only test, NOT PRD !!

    var path = dirApp + "MyData.db";

    using var db = new LiteDatabase(path);

    var sync = new Synchronize(db, new SynchronizeOptions()
    {
        PathFile = dirApp,
        Mode = Mode.DEBUG,
        SaveLogOnFile = false
    });

    var colUser = db.GetCollection<User>(colName);

    //var user = new User(Guid.NewGuid()) { Name = "Oliveira" };

    // sync.InsertSync(user, colUser);

    //multipe create

    //for (int i = 0; i < 50; i++)
    //{
    //    var user = new User(Guid.NewGuid())
    //    {
    //        Name = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}"
    //    };

    //    user.CreateAt();
    //    colUser.Insert(user);
    //}

    //var usersUpdateTeste = colUser.Query().Where(u => u.Name.StartsWith("Cotoso ")).Limit(10).ToList();

    //foreach (var us in usersUpdateTeste)
    //{
    //    us.Name = "Cotoso update";
    //    us.UpdateAt();

    //    colUser.Update(us);
    //}

    //var user = colUser.FindOne(u => u.Id == Guid.Parse(" id para deletar"));

    //user.DeleteAt();

    //colUser.Update(user);

    //var colEstoque = db.GetCollection<Estoque>(colNameEstoque);

    //for (int i = 0; i < 100; i++)
    //{
    //    var estoque = new Estoque(Guid.NewGuid())
    //    {
    //        Valor = new Random().Next(50)
    //    };

    //    estoque.CreateAt();
    //    colEstoque.Insert(estoque);
    //}

    //pull local

    var x = 0;

    var now = DateTime.UtcNow;

    var pullChangesBuilder = new SyncPullBuilder();

    pullChangesBuilder
        .AddPullSync(() => sync.PullChangesResult<User>(x, colName, now))
        .AddPullSync(() => sync.PullChangesResult<Estoque>(x, colNameEstoque, now))
        .Build();

    var databaseLocal = pullChangesBuilder.DatabaseLocalChanges;

    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(databaseLocal, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));

    //sending update database local changes [databaseLocal]
    //post api/push

    //simute server changes
    //api/pull
    //string responseServer = await File.ReadAllTextAsync(dirApp + "ResponseServer.json");

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

public class User : SchemaSync
{
    public User()
    {
    }

    public User(Guid id) : base(id)
    {
    }

    public string Name { get; set; } = string.Empty;
}

public class Estoque : SchemaSync
{
    public Estoque(Guid id) : base(id)
    {
    }

    public int Valor { get; set; }
}