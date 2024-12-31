using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Exceptions;
using SSync.Client.LitebDB.Sync;


namespace SSync.Client.LiteDB.Tests.Sync
{
    public class PullChanges_Tests
    {
        [Fact]
        public void SetDatabaseLiteDbNull_SchoudReturnPullChangesExceptions()
        {
            //arrange
  var lastPulledAt = DateTime.MinValue;
            var collectionName = "user";

            //act

            var sync = new Synchronize(null!);

            Action act = () => sync.PullChangesResult<User>(lastPulledAt, collectionName);

            //assert
            PullChangesException exception = Assert.Throws<PullChangesException>(act);

            Assert.Equal("Database not initialized", exception.Message);
        }

        [Fact(Skip = "Must fix parallel query fetch")]
        public void LastPulledAtLessZero_ShouldReturnExceptionPullChangesException()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow}";

            var lastPulledAt = DateTime.MinValue;
            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            //act
            var collection = database.GetCollection<User>(collectionName);
            collection.Insert(new User(newUserid)
            {
                Name = newUserName
            });

            var sync = new Synchronize(database);

            Action act = () => sync.PullChangesResult<User>(lastPulledAt, collectionName);

            //assert
            PullChangesException exception = Assert.Throws<PullChangesException>(act);

            Assert.Equal("Range less of zero", exception.Message);
        }

        [Fact]
        public void InsertAndUpdatedAndDeleteRowsSync_ShouldReturnPullChangesWithSameMethods()
        {
            var colUserName = "user";

            var users = Enumerable.Range(0, 4).Select(_ => new User(Guid.NewGuid())
            {
                Name = $"Cotoso {DateTime.UtcNow}"
            }).ToArray();

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            foreach (var us in users)
            {
                sync.InsertSync(us, colUserName);
            }

            sync.UpdateSync(users[0], colUserName);

            sync.DeleteSync(users[3], colUserName);

            var changes = sync.PullChangesResult<User>(DateTime.MinValue, colUserName);

            var expectedUsersCreated = 2;
            var expectedUsersUpdated = 1;
            var expectedUsersDeleted = 1;

            var totalUsersCreated = changes.Changes.Created.Count();
            var totalUsersUpdated = changes.Changes.Updated.Count();
            var totalUsersDeleted = changes.Changes.Deleted.Count();

            Assert.Equal(expectedUsersCreated, totalUsersCreated);
            Assert.Equal(expectedUsersUpdated, totalUsersUpdated);
            Assert.Equal(expectedUsersDeleted, totalUsersDeleted);
        }

        public class User(Guid id) : SchemaSync(id)
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}