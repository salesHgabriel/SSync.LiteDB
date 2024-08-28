﻿using Moq;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;
using SSync.Server.LitebDB.Sync;
using SSync.Shared.ClientServer.LitebDB.Exceptions;

namespace SSync.Server.LitebDB.Tests.Sync
{
    public class PullChanges_Tests
    {

        [Fact]
        public async Task NoSetupChanges_SchoudReturnNull()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = [],
                Timestamp = 0
            };

            var schemaMock = new Mock<ISchemaCollection>();
            
            var itens = await schemaMock.Object.PullChangesAsync(parameter);
           
            Assert.Null(itens);
        }

        [Fact]
        public async Task SetNoneCollection_SchoudReturnPullChangesException()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = [],
                Timestamp = 0
            };

            var syncServiceMock = new Mock<ISSyncServices>();
            var pullExecutationMock = new Mock<IPullExecutionOrderStep>();
            var pushExecutationMock = new Mock<IPushExecutionOrderStep>();
            var syncDbContextTransactionMock = new Mock<ISSyncDbContextTransaction>();

            pullExecutationMock.Setup(s => s.By<UserSync>("user"));

            var shemaCollection = new SchemaCollection(syncServiceMock.Object, pullExecutationMock.Object, pushExecutationMock.Object, syncDbContextTransactionMock.Object);

            async Task<List<object>> act() => await shemaCollection.PullChangesAsync(parameter);

            PullChangesException exception = await Assert.ThrowsAsync<PullChangesException>((Func<Task<List<object>>>)act);

            Assert.Equal("You need set collections", exception.Message);
        }


        [Fact]
        public async Task SetTimeStampLessThanZero_SchoudReturnPullChangesException()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = ["User"],
                Timestamp = -1
            };

            var syncServiceMock = new Mock<ISSyncServices>();
            var pullExecutationMock = new Mock<IPullExecutionOrderStep>();
            var pushExecutationMock = new Mock<IPushExecutionOrderStep>();
            var syncDbContextTransactionMock = new Mock<ISSyncDbContextTransaction>();

            pullExecutationMock.Setup(s => s.By<UserSync>("user"));

            var shemaCollection = new SchemaCollection(syncServiceMock.Object, pullExecutationMock.Object, pushExecutationMock.Object, syncDbContextTransactionMock.Object);

            async Task<List<object>> act() => await shemaCollection.PullChangesAsync(parameter);

            PullChangesException exception = await Assert.ThrowsAsync<PullChangesException>((Func<Task<List<object>>>)act);

            Assert.Equal("Timestamp should be zero or more", exception.Message);
        }
    }











    public class UserSync(Guid id) : ISchema(id)
    {
        public string? Name { get; set; }
    }
}
