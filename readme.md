# About and  how works:
SSYNC.LiteDB aims to assist in the data synchronization flow between the backend, using .NET WebAPI/Minimal APIs, and the frontend, using .NET MAUI or Uno Platform, with LiteDB as the local database.


## To update local changes:

![alt text](doc/flow_update_local_changes.png "Img Update local changes")

## To update server changes:

## ![alt text](doc/flow_update_server_changes.jpg "Img Update server changes")

## Flow (en-us):
![alt text](doc/notes_ssync_en.png "Img Flow ssynclitedb en-us")


## Flow pt-br:
![alt text](doc/notes_ssync_pt_br.png "Img Flow ssynclitedb pt-br")


⚠️⚠️ Important Notes:

* Your local and server databases must contain ID fields of type GUID, as well as fields for creation date, update date, and deletion date. To assist with this, you should use ISSyncEntityRoot in the backend, an abstract class to be used in your entities.

* Example your api server: 

```cs
// example entity from server

    public class Note : ISSyncEntityRoot
    {
        public const string CollectionName = nameof(Note);
        public Note()
        {
            
        }
        public Note(Guid id, Time time) : base(id, time)
        {
        }

        public string? Content { get; set; }
        public bool Completed { get; set; }
    }
```


*  Case your client to use  SchemaSync

```cs
// example client with .NET Maui
    public class Note : SchemaSync
    {
        public Note(Guid id) : base(id, SSync.Client.LitebDB.Enums.Time.UTC)
        {
        }

        public string? Content { get; set; }

        public bool Completed { get; set; }
    }
```
 
* Your server database você will work only soft delete.

* In your client, you must perform CRUD operations while always updating the date fields. To assist with this, Synchronize provides insert, update, and delete methods to abstract these operations. Using these methods is optional, the date updates are performed.

* Your client if not change your database litedb, you need set first pull to get all changes from server e set last pullet At

```cs
// example repository client in .NET Maui.

    public class NoteRepository : INoteRepository
    {
        private Synchronize? _sync;
        private readonly LiteDatabase? _db;

        public NoteRepository()
        {
            _db = new LiteDatabase(GetPath());
            _sync = new Synchronize(_db);
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

		.....

```

* If you do not use Synchronize to delete fields in your local database, it is important to note that you must update the data (updating the date and status) instead of deleting it. You can use methods such as entity.CreatedAt() or entity.DeletedAt().




## Client:

### How install

[![Nuget](https://img.shields.io/nuget/v/SSync.Client.LitebDB)](https://www.nuget.org/packages/SSync.Client.LitebDB/)

 *  setup
 * To use methods to sync initialize the classe as:

```cs
public class SyncRepo
{
private Synchronize? _sync;
private readonly LiteDatabase? _db;

public SyncRepo()
{
	_db = new LiteDatabase(GetPath());
	_sync = new Synchronize(_db);
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


public Task SetLastPulledAt(DateTime lastPulledAt)
{
	_sync!.ReplaceLastPulledAt(lastPulledAt);
	return Task.CompletedTask;
}
public DateTime GetLastPulledAt()
{
	return _sync!.GetLastPulledAt();
}



// ---------------  PUSH -------------------


//Load database server to my local
public Task PushServerChangesToLocal(string jsonServerChanges)
{
	var pushBuilder = new SyncPushBuilder(jsonServerChanges);

	pushBuilder
		.AddPushSchemaSync<Note>(change => _sync!.PushChangesResult(change), LiteDbCollection.Note)
		.AddPushSchemaSync<AnotherCollection>(change => _sync!.PushChangesResult(change), LiteDbCollection.AnotherNameCollection)
		.Build();

	return Task.CompletedTask;
}
	
// ---------------  PULL -------------------

	//send database local to server
public string PullLocalChangesToServer()
{
	var pullChangesBuilder = new SyncPullBuilder();

	var lastPulledAtSync = _sync!.GetLastPulledAt();

//Get Change from database litebd
	pullChangesBuilder
		.AddPullSync(() => _sync!.PullChangesResult<Note>(lastPulledAtSync, LiteDbCollection.Note))
		.AddPullSync(() => _sync!.PullChangesResult<AnotherCollection>(lastPulledAtSync, LiteDbCollection.AnotherNameCollection))
		.Build();

	var databaseLocal = pullChangesBuilder.DatabaseLocalChanges;
	var jsonDatabaseLocal = pullChangesBuilder.JsonDatabaseLocalChanges;

	return jsonDatabaseLocal;
}

```

## Sample my service and view model with flur (nuget package to work with api requests)

```cs
    public class ApiService : IApiService
    {
        private readonly ISyncRepo _syncService;

        public ApiService(SyncRepo syncService)
        {
            _syncService = syncService;
        }

        public async Task<int> PushServer()
        {

            //get local database
            var time = _syncService.GetLastPulledAt();
            var localDatabaseChanges = _syncService.PullLocalChangesToServer(time);

            //send local database to server
            var result = await "https://my-api.com"
                .AppendPathSegment("api/Sync/Push")
                .AppendQueryParam("Colletions", LiteDbCollection.Note)
                .AppendQueryParam("Colletions", LiteDbCollection.AnoterCollectionName)
                .AppendQueryParam("Timestamp", time)
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-type", "application/json")
                .PostStringAsync(localDatabaseChanges);

            var resp = await result.ResponseMessage.Content.ReadAsStringAsync();

			//always need set lastPulletAt from response to your client litedb to know last sync with your server 

             var dta = JsonSerializer.Deserialize<DateTimeOffset>(resp);
             await _syncService.SetLastPulledAt(dta.Date);
            
            return result.StatusCode;
        }

        public async Task PullServer(bool firstPull)
        {
			//if true, get all change of server
            // get server database
            var time = firstPull ? DateTime.MinValue : _syncService.GetLastPulledAt();

            var result = await "https://my-api.com"
            .AppendPathSegment("api/Sync/Pull")
            .AppendQueryParam("Colletions", LiteDbCollection.Note)
            .AppendQueryParam("Colletions", LiteDbCollection.AnoterCollectionName)
            .AppendQueryParam("Timestamp", time.ToString("o")) // to convert to iso
            .GetAsync();

            var res = await result.ResponseMessage.Content.ReadAsStringAsync();

            //update local database from server

            await _syncService.PushServerChangesToLocal(res);
        }

    }
}

	// ViewModel

    public class NoteViewModel : INotifyPropertyChanged
    {

        private readonly IApiService _apiService;

		
        private void PullChangesNow()
        {
            _apiService.PullServer(firstPull:false);
        }

        private void PullAllChanges()
        {
            _apiService.PullServer(firstPull:true);
        }


        private void PushChanges()
        {
            _apiService.PushServer();
        }

```



# Backend

### How install

[![Nuget](https://img.shields.io/nuget/v/SSync.Server.LitebDB)](https://www.nuget.org/packages/SSync.Server.LitebDB/)


* Setup


```cs
// Sample entity
public class Note : ISSyncEntityRoot
{
	public const string CollectionName = nameof(Note);
	public Note()
	{
		
	}
	public Note(Guid id, Time time) : base(id, time)
	{
	}

	public string? Content { get; set; }
	public bool Completed { get; set; }
}

// Sample program.cs


// Create Class as like dto to you client

    public class NoteSync : ISchema
    {
        public NoteSync(Guid id) : base(id)
        {
        }

        public string? Content { get; set; }
        public bool Completed { get; set; }

    }


builder.Services.AddSSyncSchemaCollection<PocDbContext>(
    (pullChangesConfig) =>
    {
        pullChangesConfig
           .By<NoteSync>(Note.CollectionName);
    }, (pushChangesConfig) =>
    {
        pushChangesConfig
           .By<NoteSync>(Note.CollectionName);
    });


//Sample DbContext
// Set interface ISSyncDbContextTransaction

  public class PocDbContext : DbContext, ISSyncDbContextTransaction
    {
        private readonly IConfiguration _configuration;
        private IDbContextTransaction? transaction;

        public PocDbContext(DbContextOptions<PocDbContext> dbContextOptions, IConfiguration configuration) : base(dbContextOptions)
        {
            _configuration = configuration;
        }

        public DbSet<Note> Notes { get; set; }


        public async Task BeginTransactionSyncAsync()
            => transaction = await Database.BeginTransactionAsync();

        public async Task CommitSyncAsync()
            => await Database.CommitTransactionAsync();

        public Task CommitTransactionSyncAsync()
        {
            ArgumentNullException.ThrowIfNull(transaction);

            return transaction.CommitAsync();
        }

        public Task RollbackTransactionSyncAsync()
        {
            ArgumentNullException.ThrowIfNull(transaction);
            return transaction.RollbackAsync();
        }

// ------------ PULL -----------------------

    public class PullNotesRequestHandler : ISSyncPullRequest<NoteSync, SSyncParameter>
    {
        private readonly PocDbContext _db;

        public PullNotesRequestHandler(PocDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<NoteSync>> QueryAsync(SSyncParameter parameter)
        {
            var notes = await _db.Notes.Select(n => new NoteSync(n.Id)
            {
                Content = n.Content,
                Completed = n.Completed,
                CreatedAt = n.CreatedAt,
                DeletedAt = n.DeletedAt,
                UpdatedAt = n.UpdatedAt
            }).ToListAsync();

            return notes;
        }
    }
}


// ------------ PUSH -----------------------

    public class PusNotesRequestHandler : ISSyncPushRequest<NoteSync>
    {
        private readonly PocDbContext _db;

        public PusNotesRequestHandler(PocDbContext db)
        {
            _db = db;
        }

        public async Task<NoteSync?> FindByIdAsync(Guid id)
        {
            return await _db.Notes
               .Where(n => n.Id == id)
               .Select(n => new NoteSync(id)
               {
                   Content = n.Content,
                   Completed = n.Completed,
                   CreatedAt = n.CreatedAt,
                   UpdatedAt = n.UpdatedAt,
                   DeletedAt = n.DeletedAt
               }).FirstOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(NoteSync schema)
        {
            var newNote = new Note(schema.Id, Time.UTC)
            {
                Content = schema.Content,
                Completed = schema.Completed
            };

            await _db.Notes.AddAsync(newNote);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(NoteSync schema)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == schema.Id);

            if (note is null)
                return false;

            note.Content = schema.Content;

            note.Completed = schema.Completed;

            note.SetUpdatedAt(DateTime.UtcNow);

            _db.Notes.Update(note);
            return await _db.SaveChangesAsync() > 0;
        }

		//Always works soft delete
        public async Task<bool> DeleteAsync(NoteSync schema)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == schema.Id);

            if (note is null)
                return false;


            note.SetDeletedAt(DateTime.UtcNow);

            _db.Notes.Update(note);
            return await _db.SaveChangesAsync() > 0;
        }
    }


    // Your Controle ou Endpoint inject ISchemaCollection to use methods pull and push

    // Controller WebApi

          [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Pull([FromQuery] SSyncParameter parameter, [FromServices] ISchemaCollection schemaCollection)
        {
            return Ok(await schemaCollection.PullChangesAsync(parameter));
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Push([FromQuery] SSyncParameter parameter, [FromBody] JsonArray changes, [FromServices] ISchemaCollection schemaCollection)
        {
            if (changes is not null)
            {
                return Ok(await schemaCollection.PushChangesAsync(changes, parameter));
            }
            return BadRequest();
        }
    
    // Minimal api
    
     app.MapGet("/pull", async ([AsParameters] PlayParamenter parameter, [FromServices] ISchemaCollection schemaCollection) =>
    {
        var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter, new SSyncOptions()
        {
            Mode = Mode.DEBUG
        });

        return Results.Ok(pullChangesRemoter);
    });

    app.MapPost("/push", async (HttpContext httpContext, JsonArray changes, [FromServices] ISchemaCollection schemaCollection) =>
    {
        var query = httpContext.Request.Query;

        var parameter = new PlayParamenter
        {
            Time = Convert.ToInt32(query["time"]),
            Colletions = query["colletions"].ToArray()!,
            Timestamp = DateTime.TryParse(query["timestamp"], out DateTime timestamp) ? timestamp : DateTime.MinValue
        };

        var isOk = await schemaCollection.PushChangesAsync(changes, parameter, new SSyncOptions()
        {
            Mode = Mode.DEBUG
        });

        return Results.Ok(isOk);
    });

``




