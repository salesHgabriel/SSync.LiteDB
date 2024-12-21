![alt text](doc/ssync_thumb.png "Img thumb ssynclitedb")

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/salesHgabriel/SSync.LiteDB/blob/master/readme.md)
[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg)](https://github.com/salesHgabriel/SSync.LiteDB/blob/master/readme.pt-br.md)

## About:
SSYNC.LiteDB aims to simplify implementing data synchronization between the frontend using LiteDB and the backend.

## ⚠️ Important Notes:
- Your local and server databases must always use: 
    - - GUID for identifiers
    - - Tables requiring data synchronization must include the columns: CreatedAt, UpdatedAt, and DeletedAt (timestamps).
    - -  The DeletedAt column is a nullable datetime, meaning you will always work with soft deletes.
    - -  The timestamp 01-01T00:00:00.0000000 (ISO) or 1/1/0001 12:00:00 AM is used as a reference to load all server data.
    - -  Data transactions must always use consistent data formats (UTC or local), both for server and client.
    - -  Data structure (schemas) must be consistent between server and client e keys names.
    - - Deleted objects are always only represented by their IDs.
   - - structure object valid
    ```json
        [
            {
                collection: table1_key_name,
                timestamp: Datetime iso like //2024-12-21T17:37:41.7618382Z ,
                changes:{
                    created: dtoSync[],
                    updated: dtoSync[],
                    deleted: string[],
                }  
            },
            {
                collection: table2_key_name,
                timestamp: Datetime iso like //2024-12-21T17:37:41.7618382Z ,
                changes:{
                    created: dtoSync[],
                    updated: dtoSync[],
                    deleted: string[],
                }  
            }
        ]
    ``` 
   - - Example

   ```json
        [
            {
                "collection": "ss_tb_user",
                "timestamp": "2024-12-21T17:37:41.7618382Z",
                "changes": {
                    "created": [
                        {
                            "name": "John Doe 45",
                            "age": 45,
                            "id": "a6ea4282-a3ea-4893-ba50-e11e673030ef",
                            "createdAt": "2024-12-21T17:25:53.961871",
                            "updatedAt": "2024-12-21T17:25:53.961871",
                            "deletedAt": null
                        },
                        {
                            "name": "John Doe 19",
                            "age": 19,
                            "id": "f878dac6-690a-4f5e-b929-5ebecb1cd434",
                            "createdAt": "2024-12-21T17:25:54.689497",
                            "updatedAt": "2024-12-21T17:25:54.689497",
                            "deletedAt": null
                        }
                    ],
                    "updated": [],
                    "deleted": [
                        "7897a793-6298-42e4-b1f9-bb6daa8fe948"
                    ]
                }
            },
            {
                "collection": "ss_tb_note",
                "timestamp": "2024-12-21T17:37:42.8157339Z",
                "changes": {
                    "created": [
                        {
                            "completed": false,
                            "message": "new note",
                            "userName": "John Doe 19",
                            "id": "66c2eb1e-14f3-4ec7-8afd-84f54a42287b",
                            "createdAt": "2024-12-21T17:26:11.350763",
                            "updatedAt": "2024-12-21T17:26:11.350763",
                            "deletedAt": null
                        },
                        {
                            "completed": false,
                            "message": "new note",
                            "userName": "John Doe 19",
                            "id": "b7c66d5b-ea12-4710-a985-436942b0e938",
                            "createdAt": "2024-12-21T17:26:16.925801",
                            "updatedAt": "2024-12-21T17:26:16.925801",
                            "deletedAt": null
                        }
                    ],
                    "updated": [],
                    "deleted": []
                }
            }
        ]
   ```    



## 🔄️ Flow
![alt text](doc/notes_ssync_en.png "Img Flow ssynclitedb en-us")


## To update local changes:

![alt text](doc/flow_update_local_changes.png "Img Update local changes")

## To update server changes:

## ![alt text](doc/flow_update_server_changes.jpg "Img Update server changes")

## Flow (en=us):
![alt text](doc/notes_ssync_en.png "Img Flow ssynclitedb en-us")

<details open>
<summary><h2>🔙 Backend</h2></summary>

### Installation


[![Nuget](https://img.shields.io/nuget/v/SSync.Server.LitebDB)](https://www.nuget.org/packages/SSync.Server.LitebDB/)

### ⛏️ Setup

1. Set Up Your Data Models: <br/>
Your model can inherit from ISSyncEntityRoot to automatically create the necessary columns for synchronization management.

```cs
// example entity from server

public class Note : ISSyncEntityRoot
{
    public const string CollectionName = "ss_tb_note";
    
    public Note()
    {
        
    }
    public Note(Guid id, Time time) : base(id, time)
    {
    }

    
    public bool Completed { get; set; }
    public string? Message { get; set; } 
    public Guid? UserId{ get; set; }
    public virtual User? User { get; set; }

}
```

2. Configure Data Transfer Object (DTO): <br/>
Define a schema that represents the synchronized data object.
```cs
// example dto to shared data

public class NoteSync : ISchema
{
    public NoteSync(Guid id) : base(id)
    {
    }

    public bool Completed { get; set; }

    public string? Message { get; set; }

    public string? UserName { get; set; }
}
```


3. Configure Your DbContext:<br/>
The DbContext must inherit from ISSyncDbContextTransaction.

```cs

//sample dbcontext with postgresql

public class PocDbContext : DbContext, ISSyncDbContextTransaction
{
    private IDbContextTransaction? _transaction;
    private readonly IConfiguration _configuration;

    public PocDbContext(DbContextOptions<PocDbContext> options, IConfiguration configuration) : base(options)
    {
        //fix :https://github.com/npgsql/npgsql/issues/4246
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }


    public async Task BeginTransactionSyncAsync()
        => _transaction = await Database.BeginTransactionAsync();

    public async Task CommitSyncAsync()
        => await Database.CommitTransactionAsync();

    public Task CommitTransactionSyncAsync()
    {
        ArgumentNullException.ThrowIfNull(_transaction);

        return _transaction.CommitAsync();
    }

    public Task RollbackTransactionSyncAsync()
    {
        ArgumentNullException.ThrowIfNull(_transaction);
        return _transaction.RollbackAsync();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PocServerSync"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(n => n.Notes)
            .OnDelete(DeleteBehavior.Restrict);


        
        modelBuilder.Entity<User>()
            .HasMany(n => n.Notes)
            .WithOne(n => n.User)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

```

4. Create a Pull Handler: <br/>
This class facilitates downloading the synchronization structure and implements the ISSyncPullRequest<ISchema, SSyncParameter> interface.

```cs
// ~/Sync.Handlers.Pull/NotePullRequestHandler.cs

public class NotePullRequestHandler : ISSyncPullRequest<NoteSync, SSyncParameter>
{
    private readonly ILogger<NotePullRequestHandler> _logger;
    private readonly PocDbContext _pocDbContext;

    public NotePullRequestHandler(ILogger<NotePullRequestHandler> logger, PocDbContext pocDbContext)
    {
        _logger = logger;
        _pocDbContext = pocDbContext;
    }

    public async Task<IEnumerable<NoteSync>> QueryAsync(SSyncParameter parameter)
    {
        _logger.LogInformation("Not sync  pull");

        var notes = _pocDbContext.Notes.AsQueryable();

        if (parameter.UserId.HasValue)
        {
            notes = notes.Where(x => x.UserId == parameter.UserId);
        }

        return await notes.Select(n => new NoteSync(n.Id)
        {
            Completed = n.Completed,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            Message = n.Message,
            DeletedAt = n.DeletedAt,
            UserName = n.User!.Name
        }).ToListAsync();
    }
}

```
5. Setup Push Handlers <br/>
Now, create your Push Handler class to assist with CRUD operations for synchronization data structures. It must implement the interface ISSyncPushRequest<ISchema>.

```cs
// ~/Sync.Handlers.Push/NotePushRequestHandler.cs

    public class NotePushRequestHandler(PocDbContext context) : ISSyncPushRequest<NoteSync>
    {
        private readonly PocDbContext _context = context;

        public async Task<NoteSync?> FindByIdAsync(Guid id)
        {
            return await _context.Notes.Where(u => u.Id == id)
                .Select(u => new NoteSync(id)
                {
                    Completed = u.Completed,
                    Message = u.Message,
                    UserName = u.User!.Name,
                    CreatedAt = u.CreatedAt,
                    DeletedAt = u.DeletedAt,
                    UpdatedAt = u.UpdatedAt
                }).FirstOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(NoteSync schema)
        {
            var userId = await _context.Users
                .Where(s => s.Name == schema.UserName)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            var newNote = new Note(schema.Id, Time.UTC)
            {
                Completed = schema.Completed,
                Message = schema.Message,
                UserId = userId
            };

            await _context.Notes.AddAsync(newNote);

            return await Save();
        }

        public async Task<bool> UpdateAsync(NoteSync schema)
        {
            var entity = await _context.Notes.FindAsync(schema.Id);

            entity!.Completed = schema.Completed;
            entity.Message = schema.Message;

            entity.SetUpdatedAt(DateTime.UtcNow);

            _context.Notes.Update(entity);

            return await Save();
        }

        public async Task<bool> DeleteAsync(NoteSync schema)
        {
            var entity = await _context.Notes.FindAsync(schema.Id);

            entity!.Completed = schema.Completed;
            entity.Message = schema.Message;

            entity.SetDeletedAt(DateTime.UtcNow);

            _context.Notes.Update(entity);

            return await Save();
        }

        private async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }

```

6. Setup your program.cs 


```cs

builder.Services.AddSSyncSchemaCollection<PocDbContext>(
    optionsPullChanges:(pullChangesConfig) =>
    {
        pullChangesConfig
            .By<UserSync>(User.CollectionName)
            .ThenBy<NoteSync>(Note.CollectionName);
    },
    optionsPushChanges: (pushChangesConfig) =>
    {
        pushChangesConfig
            .By<UserSync>(User.CollectionName)
            .ThenBy<NoteSync>(Note.CollectionName);
    });

```



7. Now, you can use the ISchemaCollection interface to perform pull or push operations in your controller or endpoint.<br/>
Here's the translated implementation for the backend:

```cs
 // endpoint
 var syncGroup = app.MapGroup("api/sync").WithOpenApi();

syncGroup.MapGet("/pull", async ([AsParameters] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection) =>
{
    var changes = await schemaCollection.PullChangesAsync(parameter);
    
    return Results.Ok(changes);
});

syncGroup.MapPost("/push", async (HttpContext httpContext, [FromBody] JsonArray changes, [FromServices] ISchemaCollection schemaCollection) =>
{
    var query = httpContext.Request.Query;

    var sucesso = Guid.TryParse(query["userId"], out var userId);
    var parameter = new SSyncParameter
    {
        UserId = sucesso ? userId : null,
        Colletions = query["collections"].ToArray()!,
        Timestamp = DateTime.TryParse(query["timestamp"], out DateTime timestamp) ? timestamp : DateTime.MinValue
    };


    var now = await schemaCollection.PushChangesAsync(changes, parameter);

    return Results.Ok(now);
});

  
 // controller
[Route("[action]")]
[HttpGet]
public async Task<IActionResult> Pull([FromQuery] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection)
{
    return Ok(await schemaCollection.PullChangesAsync(parameter));
}

[Route("[action]")]
[HttpGet]
public IAsyncEnumerable<object> PullStream([FromQuery] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection)
{
    return schemaCollection.PullStreamChanges(parameter);
}

```

8. Extend SSyncParameter to Provide Custom Parameters Available Across All Pull Handlers. <br/>
You can inherit from the SSyncParameter class to add custom fields or additional data that will be available in all your Pull Handlers. This is useful if you need to pass additional information to your synchronization logic.

```cs
public class CustomParamenterSync : SSyncParameter
{
    public Guid? UserId { get; set; }
    public string? phoneId { get; set; }
}

```
</details>

<details open>
<summary><h2>📱 Client</h2></summary>

### Installation

[![Nuget](https://img.shields.io/nuget/v/SSync.Client.LitebDB)](https://www.nuget.org/packages/SSync.Client.LitebDB/)

### ⛏️ Setup


1. Your entities must inherit from the SchemaSync class:c

```cs
    public class Note : SchemaSync
    {
        public Note(Guid id) : base(id, SSync.Client.LitebDB.Enums.Time.UTC)
        {
        }

        public string? Content { get; set; }

        public bool Completed { get; set; }
    }
```

(Optional) 1.1. Table names must be unique and match your backend:

```cs
   public static class LiteDbCollection
    {
        public const string Note = "ss_tb_note";
    }
}

```
2. Your data operations (CRUD) must use the Synchronize class:<br/>
The NoteRepository class handles CRUD operations for the Note entity, ensuring that these operations are synchronized.

```cs
    public class NoteRepository : INoteRepository
    {
        private Synchronize? _sync;
        private readonly LiteDatabase? _db;

        public NoteRepository()
        {
            _db = new LiteDatabase(GetPath());
            _sync = new Synchronize(_db);
        }

        public List<Note> GetAll()
        {
            return _db!.GetCollection<Note>().FindAll().OrderBy(s => s.CreatedAt).ToList();
        }

        public Task Save(Note note)
        {
            _sync!.InsertSync(note,"Note");

            return Task.CompletedTask;
        }

        public Task Update(Note note)
        {
            _sync!.UpdateSync(note, "Note");

            return Task.CompletedTask;
        }

        public Task Delete(Note note)
        {
            _sync!.DeleteSync(note, "Note");
            return Task.CompletedTask;
        }


        private string GetPath()
        {
            var path = FileSystem.Current.AppDataDirectory;

#if WINDOWS
            return Path.Combine(path, "litedbwin.db");
#else
            return Path.Combine(path, "litedb.db");
#endif
        }


    }

```


3. Create a synchronization repository class:<br/>
The SyncRepository class is responsible for managing synchronization between the local and server databases.

```cs


public class SyncRepository : ISyncRepository
    {
        //send database local to server
        public string PullLocalChangesToServer(DateTime lastPulledAt)
        {
            var pullChangesBuilder = new SyncPullBuilder();

            var last = _sync!.GetLastPulledAt();
            pullChangesBuilder
                .AddPullSync(() => _sync!.PullChangesResult<Note>(last, LiteDbCollection.Note))
                // if more table to get  
                .AddPullSync(() => _sync!.PullChangesResult<AnotherTable>(last, LiteDbCollection.AnotherTable))
                .Build();

            var databaseLocal = pullChangesBuilder.DatabaseLocalChanges;
            var jsonDatabaseLocal = pullChangesBuilder.JsonDatabaseLocalChanges;

            return jsonDatabaseLocal;
        }

        //Load database server to my local
        public Task PushServerChangesToLocal(string jsonServerChanges)
        {
            var pushBuilder = new SyncPushBuilder(jsonServerChanges);

            pushBuilder
                .AddPushSchemaSync<Note>(change => _sync!.PushChangesResult(change), LiteDbCollection.Note)
                   // if more table to send  
                .AddPullSync(() => _sync!.PullChangesResult<AnotherTable>(last, LiteDbCollection.AnotherTable))
                .Build();

            return Task.CompletedTask;
        }

        // save last date did changes
        public Task SetLastPulledAt(DateTime lastPulledAt)
        {
           _sync!.ReplaceLastPulledAt(lastPulledAt);
            return Task.CompletedTask;
        }

        // get last date did changes
        public DateTime GetLastPulledAt()
        {
            return _sync!.GetLastPulledAt();
        }

    }

```

4. Implement your synchronization service:<br/>
The ApiService class manages the synchronization logic, communicating with the server and updating the local database.


```cs

    public class ApiService : IApiService
    {
        private readonly SyncRepository _syncService;

        public ApiService(SyncRepository syncService)
        {
            _syncService = syncService;
        }

        public async Task<int> PushServer()
        {
            //get local database
            var time = _syncService.GetLastPulledAt();
            var localDatabaseChanges = _syncService.PullLocalChangesToServer(time);

            //send local database to server
            var result = await "https://api-backend.com"
                .AppendPathSegment("api/Sync/Push")
                .AppendQueryParam("Colletions", LiteDbCollection.Note)
                .AppendQueryParam("Timestamp", time)
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-type", "application/json")
                .PostStringAsync(localDatabaseChanges);

            var resp = await result.ResponseMessage.Content.ReadAsStringAsync();

            var dta = JsonSerializer.Deserialize<DateTimeOffset>(resp);
            await _syncService.SetLastPulledAt(dta.Date);

            return result.StatusCode;
        }

        public async Task PullServer(bool all)
        {
            // get server database
            var time = all ? DateTime.MinValue : _syncService.GetLastPulledAt();
            var result = await "https://api-backend.com"
            .AppendPathSegment("api/Sync/Pull")
            .AppendQueryParam("Colletions", LiteDbCollection.Note)
            .AppendQueryParam("Timestamp", time.ToString("o"))
            .GetAsync();

            var res = await result.ResponseMessage.Content.ReadAsStringAsync();

            //update local database from server

            await _syncService.PushServerChangesToLocal(res);
        }

    }


```

</details>



