using Moq;
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
        public async Task SetNoneCollection_ShouldReturnPullChangesException()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = [],
                Timestamp = 0
            };

            var syncServiceMock = new Mock<ISSyncServices>();
            var pullExecutionMock = new Mock<IPullExecutionOrderStep>();
            var pushExecutionMock = new Mock<IPushExecutionOrderStep>();
            var syncDbContextTransactionMock = new Mock<ISSyncDbContextTransaction>();

            pullExecutionMock.Setup(s => s.By<UserSync>("user"));

            var schemaCollection = new SchemaCollection(syncServiceMock.Object, pullExecutionMock.Object, pushExecutionMock.Object, syncDbContextTransactionMock.Object);

            async Task<List<object>> act() => await schemaCollection.PullChangesAsync(parameter);

            PullChangesException exception = await Assert.ThrowsAsync<PullChangesException>((Func<Task<List<object>>>)act);

            Assert.Equal("You need set collections", exception.Message);
        }


        [Fact]
        public async Task SetTimeStampLessThanZero_ShouldReturnPullChangesException()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = ["User"],
                Timestamp = -1
            };

            var syncServiceMock = new Mock<ISSyncServices>();
            var pullExecutionMock = new Mock<IPullExecutionOrderStep>();
            var pushExecutionMock = new Mock<IPushExecutionOrderStep>();
            var syncDbContextTransactionMock = new Mock<ISSyncDbContextTransaction>();

            pullExecutionMock.Setup(s => s.By<UserSync>("user"));

            var schemaCollection = new SchemaCollection(syncServiceMock.Object, pullExecutionMock.Object, pushExecutionMock.Object, syncDbContextTransactionMock.Object);

            async Task<List<object>> act() => await schemaCollection.PullChangesAsync(parameter);

            PullChangesException exception = await Assert.ThrowsAsync<PullChangesException>((Func<Task<List<object>>>)act);

            Assert.Equal("Timestamp should be zero or more", exception.Message);
        }

        //TODO: FINISH THIS TEST

        [Fact]
        public async Task SetTimeStampEqualZero_ShouldReturnPullChangesOnlyCreated()
        {
            var parameter = new SSyncParamenter()
            {
                Colletions = ["User"],
                Timestamp = 0
            };

            var syncServiceMock = new Mock<ISSyncServices>();
            var pullExecutionMock= new Mock<IPullExecutionOrderStep>();
            var pushExecutionMock = new Mock<IPushExecutionOrderStep>();
            var syncDbContextTransactionMock = new Mock<ISSyncDbContextTransaction>();

            //create setup memory database
            //insert rows
            //update this rows

            pullExecutionMock.Setup(s => s.By<UserSync>("user"));

            var schemaCollection = new SchemaCollection(syncServiceMock.Object, pullExecutionMock.Object, pushExecutionMock.Object, syncDbContextTransactionMock.Object);

            var changes = await schemaCollection.PullChangesAsync(parameter);

            //should return only created rows

        }


        //TODO: CREATE TEST CHECK TIMESTAMP AND RETURN CHANGE CREATE OR UPDATE OR DELETE

    }
}



    public class UserSync(Guid id) : ISchema(id)
    {
        public string? Name { get; set; }
    }

