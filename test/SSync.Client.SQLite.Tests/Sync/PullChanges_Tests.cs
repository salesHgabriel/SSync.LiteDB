using SQLite;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Exceptions;
using SSync.Client.SQLite.Sync;

namespace SSync.Client.SQLite.Tests.Sync
{
    public class PullChanges_Tests
    {
        [Fact]
        public async Task SetDatabaseSqliteNull_SchoudReturnPullChangesExceptions()
        {
            //arrange
            var lastPulledAt = DateTime.MinValue;

            var collectionName = "user";

            //act
            var sync = new Synchronize(null!);

            var act = async () => await sync.PullChangesResultAsync<TestTable>(lastPulledAt, collectionName);

            //assert
            var exception = await Assert.ThrowsAsync<PullChangesException>(act);

            Assert.Equal("Database not initialized", exception.Message);
        }

        [Fact]
        public async Task InsertAndUpdatedAndDeleteRowsSync_ShouldReturnPullChangesWithSameMethods()
        {
            //arrange

            var colUserName = "user";

            var users = Enumerable.Range(0, 4).Select(_ => new TestTable(Guid.NewGuid())
            {
                Test = $"Cotoso {DateTime.UtcNow}"
            }).ToArray();

            var database = new TestDb();

            await database.CreateTableAsync<TestTable>();

            var sync = new Synchronize(database);

            List<Task> taskUsers = [];

            foreach (var us in users)
            {
                taskUsers.Add(sync.InsertSyncAsync(us));
            }

            await Task.WhenAll(taskUsers);

            await sync.UpdateSyncAsync(users[0]);

            await sync.DeleteSyncAsync(users[3]);

            //act

            var changes = await sync.PullChangesResultAsync<TestTable>(DateTime.MinValue, colUserName);

            var expectedUsersCreated = 2;
            var expectedUsersUpdated = 1;
            var expectedUsersDeleted = 1;

            var totalUsersCreated = changes.Changes.Created.Count();
            var totalUsersUpdated = changes.Changes.Updated.Count();
            var totalUsersDeleted = changes.Changes.Deleted.Count();

            //assert

            Assert.Equal(expectedUsersCreated, totalUsersCreated);
            Assert.Equal(expectedUsersUpdated, totalUsersUpdated);
            Assert.Equal(expectedUsersDeleted, totalUsersDeleted);
        }





        private class TestTable : SchemaSync
        {
            public TestTable()
            {
            }

            public TestTable(Guid id) : base(id)
            {
            }

            public string Test { get; set; }
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