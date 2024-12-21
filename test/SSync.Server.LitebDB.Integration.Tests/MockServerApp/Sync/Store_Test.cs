namespace SSync.Server.LitebDB.Integration.Tests.MockServerApp.Sync;

/// <summary>
/// Test to operation crud
/// </summary>
public class Store_Test : IntegrationTest
{
    public Store_Test(IntegrationFixture integrationFixture) : base(integrationFixture)
    {
    }

    [Fact]
    public async Task Create_User_And_Finance_Should_Return_Success()
    {
        var createResult = await Client.GetAsync("/create");

        Assert.True(createResult.IsSuccessStatusCode);

        var resultListObjUserAndFinance = await Client.GetAsync("/list");

        Assert.True(resultListObjUserAndFinance.IsSuccessStatusCode);
    }


    [Fact]
    public async Task Set_Update_Should_Changes_UpdatedAt()
    {
        var createResult = await Client.GetAsync("/create");

        Assert.True(createResult.IsSuccessStatusCode);

        var fistUser = DbContext.User.FirstOrDefault();
        var fistFinance = DbContext.Finances.FirstOrDefault();

        var updateUserResult = await Client.GetAsync($"/user-update/{fistUser!.Id}");
        var updateFinanceResult = await Client.GetAsync($"/finance-update/{fistFinance!.Id}");
        Assert.True(updateUserResult.IsSuccessStatusCode);
        Assert.True(updateFinanceResult.IsSuccessStatusCode);

        DbContext.ChangeTracker.Clear();

        var stateUser = DbContext.User.FirstOrDefault(u => u.Id == fistUser.Id);
        var stateFinance = DbContext.Finances.FirstOrDefault(u => u.Id == fistFinance.Id);
        
        
        Assert.True(stateUser!.CreatedAt != stateUser!.UpdatedAt);
        Assert.True(stateFinance!.CreatedAt != stateFinance!.UpdatedAt);
    }

    [Fact]
    public async Task Set_Delete_Should_Changes_Set_DeleteAt()
    {
        var createResult = await Client.GetAsync("/create");

        Assert.True(createResult.IsSuccessStatusCode);

        var fistUser = DbContext.User.FirstOrDefault();
        var fistFinance = DbContext.Finances.FirstOrDefault();

        var updateUserResult = await Client.GetAsync($"/user-delete/{fistUser!.Id}");
        var updateFinanceResult = await Client.GetAsync($"/finance-delete/{fistFinance!.Id}");
        Assert.True(updateUserResult.IsSuccessStatusCode);
        Assert.True(updateFinanceResult.IsSuccessStatusCode);

        DbContext.ChangeTracker.Clear();

        var stateUser = DbContext.User.FirstOrDefault(u => u.Id == fistUser.Id);
        var stateFinance = DbContext.Finances.FirstOrDefault(u => u.Id == fistFinance.Id);

        Assert.True(stateUser!.DeletedAt.HasValue);
        Assert.True(stateFinance!.DeletedAt.HasValue);
    }
}