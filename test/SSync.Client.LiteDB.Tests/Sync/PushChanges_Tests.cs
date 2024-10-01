using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Exceptions;
using SSync.Client.LitebDB.Extensions;
using SSync.Client.LitebDB.Poco;
using SSync.Client.LitebDB.Sync;


namespace SSync.Client.LiteDB.Tests.Sync
{
    public class PushChanges_Tests
    {
        [Fact]
        public void SetDatabaseLiteDbNull_SchoudReturnPushChangesExceptions()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var now = DateTime.Now;

            //act

            var sync = new Synchronize(null!);

            Action act = () => sync.PushChangesResult(new LitebDB.Poco.SchemaPush<User>());

            //assert
            PushChangeException exception = Assert.Throws<PushChangeException>(act);

            Assert.Equal("Database not initialized", exception.Message);
        }

        [Fact]
        public void SetSchemaChangeNull_SchoudReturnPushChangesExceptions()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var now = DateTime.Now;

            //act

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            Action act = () => sync.PushChangesResult((SchemaPush<User>)null!);

            //assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(act);

            Assert.Equal("Value cannot be null. (Parameter 'schemaPush')", exception.Message);
        }

        [Fact(Skip = "Must fix parallel query fetch")]
        public void InsertAndUpdatedAndDeleteRowsSync_ShouldReturnPushChangesWithSameMethods()
        {
            var colUserName = "user";

            var users = Enumerable.Range(0, 4).Select(u => new User(Guid.NewGuid())
            {
                Name = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}"
            }).ToArray();

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            foreach (var us in users)
            {
                sync.InsertSync(us, colUserName);
            }

            sync.UpdateSync(users[0], colUserName);

            var timeChanges = 0;
            var now = DateTime.UtcNow;

            var pullLocalCliente = sync.PullChangesResult<User>(timeChanges, colUserName, now);

            var changesLocalClient = System.Text.Json.JsonSerializer.Serialize(pullLocalCliente, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            var guidFistUserCreate = pullLocalCliente.Changes.Created.Select(c => c.Id).First();

            var newDeleted = pullLocalCliente.Changes.Deleted.Append(guidFistUserCreate);
            var newCreated = pullLocalCliente.Changes.Created.Where(u => u.Id != guidFistUserCreate);

            var changesServer = new SchemaPullResult<User>(colUserName, now.ToUnixTimestamp(), new SchemaPullResult<User>.Change(newCreated, pullLocalCliente.Changes.Updated, newDeleted));

            var listChanges = System.Text.Json.JsonSerializer.Serialize(changesServer);

            var schemaPushChangesUser = System.Text.Json.JsonSerializer.Deserialize<SchemaPush<User>>(listChanges);
            var changeServerUser = sync.PushChangesResult(schemaPushChangesUser!);

            Assert.True(changeServerUser.CommitedDatabaseOperation);
        }

        public class User(Guid id) : SchemaSync(id)
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}