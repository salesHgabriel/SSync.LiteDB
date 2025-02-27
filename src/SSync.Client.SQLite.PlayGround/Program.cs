using SQLite;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Enums;
using SSync.Client.SQLite.Sync;

try
{
    var colName = nameof(User);
    var colNameEstoque = nameof(Estoque);
    var dir = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory)!.ToString())!.ToString())!.ToString()! + "\\Examples\\ResponseServer.json";


    var db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

    var result0 = await db.CreateTableAsync<User>();
    var result1 = await db.CreateTableAsync<Estoque>();

    var sync = new Synchronize(db, new SynchronizeOptions()
    {
        PathFile = Constants.DatabasePath,
        Mode = Mode.DEBUG,
        SaveLogOnFile = false
    });

    var colUser = db.Table<User>();

    //var user = new User(Guid.NewGuid()) { Name = "Oliveira" };

    //await sync.InsertSyncAsync(user);

    //multipe create

    //List<User> users = [];
    //for (int i = 0; i < 10; i++)
    //{
    //    var user = new User(Guid.NewGuid())
    //    {
    //        Name = $"Cotoso {DateTime.UtcNow}"
    //    };

    //    user.CreateAt(Time.UTC);
    //    users.Add(user);
    //}

    //await db.InsertAllAsync(users);

    //var usersUpdateTeste = await colUser.Where(u => u.Name.StartsWith("Cotoso ")).Take(5).ToListAsync();

    //foreach (var us in usersUpdateTeste)
    //{
    //    us.Name = "Cotoso update";
    //    us.UpdateAt(Time.UTC);

    //    await db.UpdateAsync(us);
    //}

    //var idToDelete = Guid.Parse("d6f97749-c2c2-4f00-8376-3f40ff1c8d30");

    //var userTestDelete = await colUser.FirstOrDefaultAsync(u => u.Id == idToDelete);

    //userTestDelete.DeleteAt(Time.UTC);

    //await db.UpdateAsync(userTestDelete);

    //var colEstoque = db.GetCollection<Estoque>(colNameEstoque);

    //for (int i = 0; i < 15; i++)
    //{
    //    var estoque = new Estoque(Guid.NewGuid())
    //    {
    //        Valor = new Random().Next(50)
    //    };

    //    estoque.CreateAt();
    //    colEstoque.Insert(estoque);
    //}

    //pull local

    //var x = DateTime.MinValue;

    //var now = DateTime.UtcNow;


    //var pullChangesBuilder = new SyncPullBuilder();

    //await pullChangesBuilder
    //    .AddPullSync(async () => await sync.PullChangesResultAsync<User>(x, colName))
    //    .AddPullSync(async () => await sync.PullChangesResultAsync<Estoque>(x, colNameEstoque))
    //    .BuildAsync();

    //var databaseLocal = pullChangesBuilder.DatabaseLocalChanges;
    //var jsonResp = System.Text.Json.JsonSerializer.Serialize(databaseLocal, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    //Console.WriteLine(jsonResp);
    //await File.WriteAllTextAsync(dir, jsonResp);

    //Console.WriteLine(pullChangesBuilder.GetTimestampFromJson());

    //sending update database local changes [databaseLocal]
    //post api/push

    //simulate server changes and update database local
    //like api/pull
    //edit file ResponseServer.json after check changes in database 

    string responseServer = await File.ReadAllTextAsync(dir);

    // set true because property is Collection
    var pushBuilder = new SyncPushBuilder(responseServer, true);

    await pushBuilder
            .AddPushSchemaSync<User>(sync.PushChangesResultAsync<User>, colName)
            .AddPushSchemaSync<Estoque>(sync.PushChangesResultAsync<Estoque>, colNameEstoque)
            .BuildAsync();
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
    public Estoque()
    {
    }

    public Estoque(Guid id) : base(id)
    {
    }

    public int Valor { get; set; }
}

internal class Constants
{
    public const string DatabaseFilename = "TodoSQLite.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

    public static string DatabasePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "meu_banco.db");
}