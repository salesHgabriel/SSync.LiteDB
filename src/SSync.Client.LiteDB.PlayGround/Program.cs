using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Sync;
using System.Text.Json;


try
{
    var colName = nameof(User); 
    using var db = new LiteDatabase("");

    var sync = new Synchronize(db);

    //var col2 = db.GetCollection<User>(colName);


    //var user = new User(Guid.NewGuid())
    //{
    //    Name = $"Gabriel Sales {DateTime.UtcNow.Ticks}"
    //};



    //user.CreateAt();
    //col2.Insert(user);


    //col2.Insert(user2);


    //var user = col2.FindOne(u => u.Id == Guid.Parse("2a09e8f4-52d7-45de-a4f3-5f494fd036b0"));

    //user.Name = "Gabriel atualizado 3";



    //user.UpdateAt();

    //col2.Update(user);


    //user.DeleteAt();



    //var pullChangesUser = sync.PullChangesResult<User>(0/*DateTime.UtcNow.Ticks*/, colName);
    //var pullChangesEstoque = sync.PullChangesResult<Estoque>(0/*DateTime.UtcNow.Ticks*/, nameof(Estoque));

    //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(pullChangesUser, new JsonSerializerOptions() { WriteIndented = true }));


    var pullChangesBuilder = new SyncPullBuilder();

    var x = 0;

    var now = DateTime.UtcNow;

    pullChangesBuilder
        .AddSync(() => sync.PullChangesResult<User>(x, colName, now))
        .AddSync(() => sync.PullChangesResult<Estoque>(x, nameof(Estoque), now))
        .Build();

    var databaseLocal  = pullChangesBuilder.DatabaseChanges;

    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(databaseLocal, new JsonSerializerOptions() { WriteIndented = true }));



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