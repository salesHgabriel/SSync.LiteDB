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
            
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            //assert

            var user = sync.FindByIdSync<User>(newUserid, documentName);

            Assert.True(user.CreatedAt > 0);
        }

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
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            //assert

            var changes = sync.PullChangesResult<User>(0, documentName, DateTime.UtcNow);

            var userCreated = changes.Changes.Created.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userCreated!.Status == StatusSync.CREATED);
        }

        [Fact]
        public void UpdateDocumentSync_ShouldReturnCreatedAtMoreZero()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            var user = sync.FindByIdSync<User>(newUserid, documentName);

            sync.UpdateSync(user, documentName);

            //assert

            var changes = sync.PullChangesResult<User>(0, documentName, DateTime.UtcNow);

            var userUpdated = changes.Changes.Updated.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userUpdated!.UpdatedAt > 0);
        }

        [Fact]
        public void UpdateDocumentSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            var user = sync.FindByIdSync<User>(newUserid, documentName);

            sync.UpdateSync(user, documentName);

            //assert

            var changes = sync.PullChangesResult<User>(0, documentName, DateTime.UtcNow);

            var userUpdated = changes.Changes.Updated.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(user.Status == StatusSync.UPDATED);
        }


        [Fact]
        public void DeleteDocumentSync_ShouldReturnGuidDiffentEmpty()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            var user = sync.FindByIdSync<User>(newUserid, documentName);

            sync.DeleteSync(user, documentName);

            //assert

            var changes = sync.PullChangesResult<User>(0, documentName, DateTime.UtcNow);

            var userDeleted = changes.Changes.Deleted.FirstOrDefault(g => g == newUserid);

            Assert.True(userDeleted != Guid.Empty);
        }

        [Fact]
        public void DeleteDocumentSync_ShouldReturnStatusDeleted()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.Ticks}";

            var documentName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, documentName);

            var user = sync.FindByIdSync<User>(newUserid, documentName);

            sync.DeleteSync(user, documentName);

            //assert

            Assert.True(user.Status == StatusSync.DELETED);
        }

        [Fact]
        public void InsertAndUpdatedAndDeleteRowsSync_ShouldReturnPullChangesWithSameMethods()
        {
       
            var colUserName = "user";

            var users = Enumerable.Range(0, 4).Select(u => new User(Guid.NewGuid())
            {
                Name = $"Cotoso {DateTime.UtcNow.Ticks}"
            }).ToArray();

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            foreach (var us in users)
            {
                sync.InsertSync(us,colUserName);
            }
            
            sync.UpdateSync(users[0], colUserName);

            sync.DeleteSync(users[3], colUserName);

            var changes = sync.PullChangesResult<User>(0, colUserName, DateTime.UtcNow);

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

        public class User(Guid id) : BaseSync(id)
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}