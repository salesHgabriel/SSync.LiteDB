using LiteDB;
using SSync.Client.LitebDB.Abstractions.Exceptions;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Sync;

namespace SSync.Client.LiteDB.Tests.Sync
{
    public class PullChanges_Tests
    {
        [Fact]
        public void SetDatabaseLiteDbNull_SchoudReturnPullChangesExceptions()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var lastPulledAt = -1;
            var documentName = "user";
            var now = DateTime.Now;

            //act

            var sync = new Synchronize(null);

            Action act = () => sync.PullChangesResult<User>(lastPulledAt, documentName, now);

            //assert
            PullChangesException exception = Assert.Throws<PullChangesException>(act);

            Assert.Equal("Database not initialized", exception.Message);
        }

        [Fact]
        public void LastPulledAtLessZero_ShouldReturnExceptionPullChangesException()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var lastPulledAt = -1;
            var documentName = "user";
            var now = DateTime.Now;

            using var database = new LiteDatabase(new MemoryStream());

            //act
            var collection = database.GetCollection<User>(documentName);
            collection.Insert(new User(newUserid)
            {
                Name = newUserName
            });

            var sync = new Synchronize(database);

            Action act = () => sync.PullChangesResult<User>(lastPulledAt, documentName, now);

            //assert
            PullChangesException exception = Assert.Throws<PullChangesException>(act);

            Assert.Equal("Range less of zero", exception.Message);
        }

        //insert
        [Fact]
        public void InsertDocumentSync_ShouldReturnCreatedAtMoreZero()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            var collection = database.GetCollection<User>(documentName);
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collection);

            //assert

            var user = collection.FindById(newUserid);

            Assert.True(user.CreatedAt > 0);
        }


        //insert
        [Fact]
        public void InsertDocumentSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            var collection = database.GetCollection<User>(documentName);
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collection);

            //assert

            var user = collection.FindById(newUserid);

            Assert.True(user.Status == StatusSync.CREATED);
        }


        //TODO: testing the update/delete operations, method was called to update the dates

        public class User(Guid id) : BaseSync(id)
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}