using SQLite;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Sync;

namespace SSync.Client.SQLite.Tests.Sync
{
    public class WrapperPersistentSync_Tests
    {
        [Fact]
        public async Task InsertCollectionSync_ShouldReturnCreatedAtMoreMinDateTime()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var database = new TestDb();

            await database.CreateTableAsync<TestTable>();

            var sync = new Synchronize(database);

            //act

            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            //assert

            var user = await sync.FindByIdSyncAsync<TestTable>(newUserid);

            Assert.True(user.CreatedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task InsertCollectionSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var collectionName = "user";

            var database = new TestDb();
            await database.CreateTableAsync<TestTable>();



            var sync = new Synchronize(database);

            //act
            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            //assert

            var changes = await sync.PullChangesResultAsync<TestTable>(DateTime.MinValue, collectionName);

            var userCreated = changes.Changes.Created.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userCreated!.Status == StatusSync.CREATED);
        }

        [Fact]
        public async Task UpdateCollectionSync_ShouldReturnCreatedAtMoreDateMin()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var collectionName = "user";

            var database = new TestDb();
            await database.CreateTableAsync<TestTable>();


            var sync = new Synchronize(database);

            //act
            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            var user = await sync.FindByIdSyncAsync<TestTable>(newUserid);

            await sync.UpdateSyncAsync(user);

            //assert

            var changes = await sync.PullChangesResultAsync<TestTable>(DateTime.MinValue, collectionName);

            var userUpdated = changes.Changes.Updated.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userUpdated!.UpdatedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task UpdateCollectionSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var database = new TestDb();
            await database.CreateTableAsync<TestTable>();


            var sync = new Synchronize(database);

            //act
            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            var user = await sync.FindByIdSyncAsync<TestTable>(newUserid);

            await sync.UpdateSyncAsync(user);

            //assert

            Assert.True(user.Status == StatusSync.UPDATED);
        }

        [Fact]
        public async Task DeleteCollectionSync_ShouldReturnGuidDiffentEmpty()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var collectionName = "user";

            var database = new TestDb();
            await database.CreateTableAsync<TestTable>();


            var sync = new Synchronize(database);

            //act
            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            var user = await sync.FindByIdSyncAsync<TestTable>(newUserid);

            await sync.DeleteSyncAsync(user);

            //assert

            var changes = await sync.PullChangesResultAsync<TestTable>(DateTime.MinValue, collectionName);

            var userDeleted = changes.Changes.Deleted.FirstOrDefault(g => g == newUserid);

            Assert.True(userDeleted != Guid.Empty);
        }

        [Fact]
        public async Task DeleteCollectionSync_ShouldReturnStatusDeleted()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var database = new TestDb();
            await database.CreateTableAsync<TestTable>();


            var sync = new Synchronize(database);

            //act
            await sync.InsertSyncAsync(new TestTable(newUserid)
            {
                Test = newUserName
            });

            var user = await sync.FindByIdSyncAsync<TestTable>(newUserid);

            await sync.DeleteSyncAsync(user);

            //assert

            Assert.True(user.Status == StatusSync.DELETED);
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