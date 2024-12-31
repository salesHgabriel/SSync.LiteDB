using System.Text.Json;
using System.Text.Json.Nodes;
using SSync.Server.LitebDB.Poco;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

namespace SSync.Server.LitebDB.Integration.Tests.MockServerApp.Sync.Handlers
{
    /// <summary>
    /// Create test sync pull request handlers
    /// </summary>
    public class PullRequest_Test : IntegrationTest
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public PullRequest_Test(IntegrationFixture integrationFixture) : base(integrationFixture)
        {
            ArgumentNullException.ThrowIfNull(integrationFixture.Client);
        }

        //pull all test -> expected all register created and update and deleted empty ✅
        
        [Fact]
        public async Task SetTimestamp_To_PullAllChanges_Should_Return_Only_Created_Values()
        {
            var createResult = await Client.GetAsync("/create");

            Assert.True(createResult.IsSuccessStatusCode);
            
            var pullResult = await Client.GetAsync($"/pull?Time=0&Colletions=User&Timestamp={DateTime.MinValue:o}", CancellationToken.None);
            Assert.True(pullResult.IsSuccessStatusCode);
             
            var content = await pullResult.Content.ReadAsStringAsync();
            
            var data = JsonSerializer.Deserialize<JsonArray>(content);
                
            Assert.True(data!.Any());

          var usersChanges = data!.FirstOrDefault().Deserialize<SchemaPush<UserSync>>(_jsonSerializerOptions);

            Assert.NotNull(usersChanges);
            Assert.True(usersChanges.HasCreated);
            Assert.False(usersChanges.HasUpdated);
            Assert.False(usersChanges.HasDeleted);
        }
        
        
        //pull date current with data -> expect some data create ✅

        [Fact]
        public async Task Set_Current_Timestamp_To_PullChanges_Should_Return_Value_From_Current_DateTime()
        {
            var currentTime = DateTime.UtcNow;

            var createResult = await Client.GetAsync("/create");

            Assert.True(createResult.IsSuccessStatusCode);

            var pullResult = await Client.GetAsync($"/pull?Time=0&Colletions=User&Timestamp={currentTime:o}", CancellationToken.None);
            Assert.True(pullResult.IsSuccessStatusCode);
             
            var content = await pullResult.Content.ReadAsStringAsync();
            
            var data = JsonSerializer.Deserialize<JsonArray>(content);
                
            Assert.True(data!.Any());

            var usersChanges = data!.FirstOrDefault().Deserialize<SchemaPush<UserSync>>(_jsonSerializerOptions);

            Assert.NotNull(usersChanges);
            Assert.True(usersChanges.HasChanges);
            Assert.True(usersChanges.HasCreated);
        }
        
    }
}
