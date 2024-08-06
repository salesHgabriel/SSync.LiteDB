﻿using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Sync;
using SSync.Shared.ClientServer.LitebDB.Extensions;
using static SSync.Client.LiteDB.Tests.Sync.PullChanges_Tests;

namespace SSync.Client.LiteDB.Tests.Sync
{
    public class WrapperPersistentSync_Tests
    {
        [Fact]
        public void InsertCollectionSync_ShouldReturnCreatedAtMoreZero()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act

            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            //assert

            var user = sync.FindByIdSync<User>(newUserid, collectionName);

            Assert.True(user.CreatedAt > 0);
        }

        [Fact]
        public void InsertCollectionSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            //assert

            var changes = sync.PullChangesResult<User>(0, collectionName, DateTime.UtcNow);

            var userCreated = changes.Changes.Created.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userCreated!.Status == StatusSync.CREATED);
        }

        [Fact]
        public void UpdateCollectionSync_ShouldReturnCreatedAtMoreZero()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            var user = sync.FindByIdSync<User>(newUserid, collectionName);

            sync.UpdateSync(user, collectionName);

            //assert

            var changes = sync.PullChangesResult<User>(0, collectionName, DateTime.UtcNow);

            var userUpdated = changes.Changes.Updated.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(userUpdated!.UpdatedAt > 0);
        }

        [Fact]
        public void UpdateCollectionSync_ShouldReturnStatusCreated()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            var user = sync.FindByIdSync<User>(newUserid, collectionName);

            sync.UpdateSync(user, collectionName);

            //assert

            var changes = sync.PullChangesResult<User>(0, collectionName, DateTime.UtcNow);

            var userUpdated = changes.Changes.Updated.FirstOrDefault(u => u.Id == newUserid);

            Assert.True(user.Status == StatusSync.UPDATED);
        }

        [Fact]
        public void DeleteCollectionSync_ShouldReturnGuidDiffentEmpty()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            var user = sync.FindByIdSync<User>(newUserid, collectionName);

            sync.DeleteSync(user, collectionName);

            //assert

            var changes = sync.PullChangesResult<User>(0, collectionName, DateTime.UtcNow);

            var userDeleted = changes.Changes.Deleted.FirstOrDefault(g => g == newUserid);

            Assert.True(userDeleted != Guid.Empty);
        }

        [Fact]
        public void DeleteCollectionSync_ShouldReturnStatusDeleted()
        {
            //arrange
            var newUserid = Guid.NewGuid();
            var newUserName = $"Cotoso {DateTime.UtcNow.ToUnixTimestamp()}";

            var collectionName = "user";

            using var database = new LiteDatabase(new MemoryStream());

            var sync = new Synchronize(database);

            //act
            sync.InsertSync(new User(newUserid)
            {
                Name = newUserName
            }, collectionName);

            var user = sync.FindByIdSync<User>(newUserid, collectionName);

            sync.DeleteSync(user, collectionName);

            //assert

            Assert.True(user.Status == StatusSync.DELETED);
        }
    }
}