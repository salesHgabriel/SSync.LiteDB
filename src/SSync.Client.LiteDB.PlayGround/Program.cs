using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Sync;
using System.Reflection;
using System.Text.Json;


try
{
    var colName = nameof(User); 
    var colNameEstoque = nameof(Estoque);

    string dirApp = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()).ToString(); // Only test, NOT PRD !!

    var path = dirApp +  "\\Examples\\MyData.db";
    
    using var db = new LiteDatabase(path);

    var sync = new Synchronize(db);

    var colUser = db.GetCollection<User>(colName);


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






    //var pullChangesUser = sync.PullChangesResult<User>(0/*DateTime.UtcNow.Ticks*/, colName);
    //var pullChangesEstoque = sync.PullChangesResult<Estoque>(0/*DateTime.UtcNow.Ticks*/, nameof(Estoque));

    //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(pullChangesUser, new JsonSerializerOptions() { WriteIndented = true }));



    var x = 0;

    var now = DateTime.UtcNow;

    var pullChangesBuilder = new SyncPullBuilder();

    pullChangesBuilder
        .AddSync(() => sync.PullChangesResult<User>(x, colName, now))
        .AddSync(() => sync.PullChangesResult<Estoque>(x, colNameEstoque, now))
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