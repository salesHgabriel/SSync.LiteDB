using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SSync.Server.LitebDB.Poco;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

namespace SSync.Server.LitebDB.Integration.Tests.MockServerApp.Sync.Handlers;

/// <summary>
/// Create test sync push request handlers
/// </summary>
public class PushRequest_Test : IntegrationTest
{
    public PushRequest_Test(IntegrationFixture integrationFixture) : base(integrationFixture)
    {
    }

    [Fact]
    public async Task Create_New_Users_From_Json_Should_Return_Created_Users_Database()
    {
        var jsonChangesFromClient = """
                                    [
                                        {
                                            "collection": "User",
                                            "timestamp": "2024-12-31T16:58:51.5566008Z",
                                            "changes": {
                                                "created": [
                                                {
                                                    "name": " Cotoso 589970329",
                                                    "id": "1851d7ed-ea2b-4b17-8e77-ab6fb2cb8f81",
                                                    "createdAt": "2024-12-31T16:58:51.172117",
                                                    "updatedAt": "2024-12-31T16:58:51.172117",
                                                    "deletedAt": null
                                                }
                                                ],
                                                "updated": [],
                                                "deleted": []
                                            }
                                        }
                                    ]
                                    """;

        var content = new StringContent(jsonChangesFromClient, Encoding.UTF8, "application/json");

        var pullResult =
            await Client.PostAsync($"/push?Time=0&Colletions=User&Timestamp={DateTime.MinValue:o}", content);
        Assert.True(pullResult.IsSuccessStatusCode);

        var user = DbContext.User.FirstOrDefault(u => u.Id == Guid.Parse("1851d7ed-ea2b-4b17-8e77-ab6fb2cb8f81"));

        Assert.NotNull(user);
    }

    
    [Fact]
    public async Task Update_Users_From_Json_Should_Return_Updated_Users_Database()
    {
        var createResult = await Client.GetAsync("/create");

        Assert.True(createResult.IsSuccessStatusCode);

        var user = DbContext.User.FirstOrDefault()!;

        var currentTime = DateTime.UtcNow;
        user.Name = "Cotoso updated";
        user.SetUpdatedAt(currentTime);

        DbContext.User.Update(user);
        await DbContext.SaveChangesAsync();

        var changes = new List<SchemaPush<UserSync>>
        {
            Capacity = 0
        };

        var updateUserSchemaSync = new SchemaPush<UserSync>
        {
            Collection = "User",
            Timestamp = currentTime,
            Changes = new SchemaPush<UserSync>.Change()
            {
                Created = [],
                Updated =
                [
                    new UserSync(user.Id)
                    {
                        Name = user.Name,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    }
                ],
                Deleted = []
            }
        };

        changes.Add(updateUserSchemaSync);


        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        var jsonChangesFromClient = JsonSerializer.Serialize(changes, serializeOptions);

        var content = new StringContent(jsonChangesFromClient, Encoding.UTF8, "application/json");

        var pullResult =
            await Client.PostAsync($"/push?Time=0&Colletions=User&Timestamp={DateTime.MinValue:o}", content);
        Assert.True(pullResult.IsSuccessStatusCode);

        DbContext.ChangeTracker.Clear();

        var userUpdate = DbContext.User.FirstOrDefault(u => u.Id == user.Id)!;

        Assert.NotEqual(userUpdate.CreatedAt, userUpdate.UpdatedAt);
    }
    
    [Fact]
    public async Task Delete_Users_From_Json_Should_Return_Delete_Users_Database()
    {
        var createResult = await Client.GetAsync("/create");

        Assert.True(createResult.IsSuccessStatusCode);

        var user = DbContext.User.FirstOrDefault()!;

        var currentTime = DateTime.UtcNow;
        user.SetDeletedAt(currentTime);

        DbContext.User.Update(user);
        await DbContext.SaveChangesAsync();

        var changes = new List<SchemaPush<UserSync>>
        {
            Capacity = 0
        };

        var deleteUserSchemaSync = new SchemaPush<UserSync>
        {
            Collection = "User",
            Timestamp = currentTime,
            Changes = new SchemaPush<UserSync>.Change()
            {
                Created = [],
                Updated = [],
                Deleted = [user.Id]
            }
        };

        changes.Add(deleteUserSchemaSync);


        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        var jsonChangesFromClient = JsonSerializer.Serialize(changes, serializeOptions);

        var content = new StringContent(jsonChangesFromClient, Encoding.UTF8, "application/json");

        var pullResult =
            await Client.PostAsync($"/push?Time=0&Colletions=User&Timestamp={DateTime.MinValue:o}", content);
        Assert.True(pullResult.IsSuccessStatusCode);

        DbContext.ChangeTracker.Clear();

        var userDeleted = DbContext.User.FirstOrDefault(u => u.Id == user.Id)!;

        Assert.NotNull(userDeleted.DeletedAt);
    }
    
    
}