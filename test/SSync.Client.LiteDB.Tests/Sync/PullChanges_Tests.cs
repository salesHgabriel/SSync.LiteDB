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

        //TODO: testing the insert/update/delete operations, method was called to update the dates









        public class User(Guid id) : BaseSync(id)
        {
            public string Name { get; set; } = string.Empty;
        }

    }
}
