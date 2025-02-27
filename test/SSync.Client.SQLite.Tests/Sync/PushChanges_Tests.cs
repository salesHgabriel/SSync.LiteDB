using SQLite;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Exceptions;
using SSync.Client.SQLite.Poco;
using SSync.Client.SQLite.Sync;

namespace SSync.Client.SQLite.Tests.Sync
{
    public class PushChanges_Tests
    {
        [Fact]
        public async Task SetDatabaseSqliteNull_SchoudReturnPushChangesExceptions()
        {
            //arrange
            SQLiteAsyncConnection? initializationDb = null;

            //act

            var sync = new Synchronize(initializationDb!);

            var act = () => sync.PushChangesResultAsync(new SchemaPush<TestTable>());

            //assert
            PushChangeException exception = await Assert.ThrowsAsync<PushChangeException>(act);

            Assert.Equal("Database not initialized", exception.Message);
        }


        [Fact]
        public async Task SetSchemaChangeNull_SchoudReturnPushChangesExceptions()
        {
            //arrange
            SchemaPush<TestTable>? dtoSchema = null;

            //act

            var database = new TestDb();

            var sync = new Synchronize(database);

            var act = () => sync.PushChangesResultAsync(dtoSchema!);

            //assert
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(act);

            Assert.Equal("Value cannot be null. (Parameter 'schemaPush')", exception.Message);
        }


        [Fact]
        public async Task InsertAndUpdatedAndDeleteRowsSync_ShouldReturnPushChangesWithSameMethods()
        {
            var colUserName = "user";

            var users = Enumerable.Range(0, 4).Select(_ => new TestTable(Guid.NewGuid())
            {
                Test = $"Cotoso {DateTime.UtcNow}"
            }).ToArray();

            var database = new TestDb();

            await database.CreateTableAsync<TestTable>();

            var sync = new Synchronize(database, new SynchronizeOptions() { Mode = Enums.Mode.DEBUG});

            List<Task> taskUsers = [];

            foreach (var us in users)
            {
                taskUsers.Add(sync.InsertSyncAsync(us));
            }

            await Task.WhenAll(taskUsers);

           await sync.UpdateSyncAsync(users[0]);

            var timeChanges = DateTime.MinValue;

            var now = DateTime.UtcNow;

            var pullLocalCliente = await sync.PullChangesResultAsync<TestTable>(timeChanges, colUserName);

            var changesLocalClient = System.Text.Json.JsonSerializer.Serialize(pullLocalCliente, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            var guidFistUserCreate = pullLocalCliente.Changes.Created.Select(c => c.Id).First();

            var newDeleted = pullLocalCliente.Changes.Deleted.Append(guidFistUserCreate);
            var newCreated = pullLocalCliente.Changes.Created.Where(u => u.Id != guidFistUserCreate);

            var changesServer = new SchemaPullResult<TestTable>(colUserName, now, new SchemaPullResult<TestTable>.Change(newCreated, pullLocalCliente.Changes.Updated, newDeleted));

            var listChanges = System.Text.Json.JsonSerializer.Serialize(changesServer);

            var schemaPushChangesUser = System.Text.Json.JsonSerializer.Deserialize<SchemaPush<TestTable>>(listChanges);

            var changeServerUser = await sync.PushChangesResultAsync(schemaPushChangesUser!);

            await WaitUntil(() => changeServerUser.CommitedDatabaseOperation, TimeSpan.FromSeconds(5));

            Assert.True(changeServerUser.CommitedDatabaseOperation);
        }

    
        private async Task WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;

            while (!condition())
            {
                if (DateTime.UtcNow - startTime > timeout)
                {
                    throw new TimeoutException("The condition was not met within the time limit.");
                }

                await Task.Delay(50); 
            }
        }


        private class TestTable : SchemaSync
        {
            public TestTable()
            {
            }

            public TestTable(Guid id) : base(id)
            {
            }

            public string Test { get; set; } = string.Empty;
        }

        public class TestDb : SQLiteAsyncConnection
        {
            public TestDb()
                : base(TestPath.GetTempFileName())
            {
                Trace = true;
            }
        }

        public class TestPath
        {
            public static string GetTempFileName()
            {
#if NETFX_CORE
			var name = Guid.NewGuid () + ".sqlite";
			return Path.Combine (Windows.Storage.ApplicationData.Current.LocalFolder.Path, name);
#else
                return Path.GetTempFileName();
#endif
            }
        }
    }
}
