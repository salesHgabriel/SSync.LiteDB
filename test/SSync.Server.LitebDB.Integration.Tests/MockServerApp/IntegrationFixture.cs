using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSync.Server.LiteDB.PlayGround.Data;
using Testcontainers.PostgreSql;

namespace SSync.Server.LitebDB.Integration.Tests.MockServerApp;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("ssync-litedb-db-test")
            .WithPortBinding(49111, 5432)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithName("ct_postgres_ssync_litedb_db_test")
            .WithImage("postgres:16")
            .Build();

    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        App = new MockServerApp(_postgreSqlContainer.GetConnectionString());
        Client = App.CreateClient();
    }

    public MockServerApp App { get; set; }
    public HttpClient Client { get; set; }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }
    
    public class  MockServerApp : WebApplicationFactory<Program>
    {
        private readonly string _postgresqlConnectionString;

        public MockServerApp(string postgresqlConnectionString)
        {
            _postgresqlConnectionString = postgresqlConnectionString;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
           
                var descriptorType =
                    typeof(DbContextOptions<TestDbContext>);

                var descriptor = services
                    .SingleOrDefault(s => s.ServiceType == descriptorType);

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<TestDbContext>(options =>
                    options.UseNpgsql(_postgresqlConnectionString));
            });
        }
    }
}

[CollectionDefinition(nameof(IntegrationFixtureCollection))]
public class IntegrationFixtureCollection : ICollectionFixture<IntegrationFixture>{ }

[Collection(nameof(IntegrationFixtureCollection))]
public class IntegrationTest : IAsyncLifetime
{
    public IntegrationTest(IntegrationFixture integrationFixture )
    {
        IntegrationFixture = integrationFixture;
    }

    public IntegrationFixture IntegrationFixture { get; }
    public HttpClient Client => IntegrationFixture.Client;
    public IServiceScope Scope { get; set; }
//    public IServiceProvider Services => Scope.ServiceProvider; case teste service with interfaces
    public TestDbContext DbContext { get; set; }
    

    public Task InitializeAsync()
    {
        Scope = IntegrationFixture.App.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<TestDbContext>();
        return Task.CompletedTask;
    }


    public Task DisposeAsync()
    {
        Scope.Dispose();
        DbContext.Dispose();
        return Task.CompletedTask;
    }
}