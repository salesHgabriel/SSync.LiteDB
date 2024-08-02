using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Sync;
using static SSync.Client.LiteDB.Tests.Sync.PullChanges_Tests;

namespace SSync.Client.LiteDB.Tests.Sync
{
    public class WrapperPersistentSync_Tests
    {
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
    }
}