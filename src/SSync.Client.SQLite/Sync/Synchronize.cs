using SQLite;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Enums;
using SSync.Client.SQLite.Exceptions;
using SSync.Client.SQLite.Extensions;
using SSync.Client.SQLite.Poco;
using System.Text;
using System.Threading.Tasks;

namespace SSync.Client.SQLite.Sync
{
    public class Synchronize : ISynchronize, IDisposable
    {
        private readonly SQLiteAsyncConnection _db;
        private SynchronizeOptions? _options;
        private bool _disposedValue;

        public Synchronize(SQLiteAsyncConnection db, SynchronizeOptions? options = null)
        {
            _db = db;
            _options = options;
        }

        /// <summary>
        /// if lastPulledAt is equal 0, load all rows for default
        /// if collectionName is null, get name of class  for default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lastPulledAt"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public async Task<SchemaPullResult<T>> PullChangesResultAsync<T>(DateTime lastPulledAt, string collectionName) where T : SchemaSync, new()
        {
            if (_db is null)
            {
                Log($"Database not initialized");
                throw new PullChangesException("Database not initialized");
            }

            Log("Start fetch local changes");

            collectionName ??= typeof(T).Name;
            var timestamp = _options?.Time == Time.LOCAL_TIME ? DateTime.Now : DateTime.UtcNow;
            var doc = _db.Table<T>();

            if (doc is null)
            {
                Log($"Collection not found");

                throw new PullChangesException("Collection not found");
            }

            var createdQuery = doc.Where(d => d.Status == StatusSync.CREATED);

            var updatedQuery = doc.Where(d => d.Status == StatusSync.UPDATED);

            var deletedQuery = doc.Where(d => d.Status == StatusSync.DELETED);

            if (lastPulledAt.IsFirstPull())
            {
                var createds = (await createdQuery.ToListAsync()).AsEnumerable();

                var updateds = (await updatedQuery.ToListAsync()).AsEnumerable();

                var deleteds = (await deletedQuery.ToListAsync())
                                .Select(d => d.Id)
                                .ToList()
                                .AsEnumerable();

                Log("Successfully fetch local all changes");

                return new SchemaPullResult<T>(collectionName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
            }

            return new SchemaPullResult<T>(
                collectionName,
                timestamp,
                new SchemaPullResult<T>.Change(

                    created: await createdQuery
                                    .Where(d => d.CreatedAt >= lastPulledAt)
                                    .Where(d => d.DeletedAt == null)
                                    .ToListAsync(),

                    updated: await updatedQuery
                                    .Where(d => d.UpdatedAt >= lastPulledAt)
                                    .Where(d => d.DeletedAt == null)
                                    .ToListAsync(),
                    
                    deleted: (await deletedQuery
                                    .Where(d => d.DeletedAt >= lastPulledAt)
                                    .Where(d => d.DeletedAt != null)
                                    .ToListAsync())
                                    .Select(d => d.Id)
                                    .Distinct()
                ));
        }

        /// <summary>
        /// Wrapper to use search collection from id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public async Task<T> FindByIdSyncAsync<T>(Guid id) where T : SchemaSync, new()
        {
            ArgumentNullException.ThrowIfNull(id);

            AsyncTableQuery<T> col = _db.Table<T>();
            ArgumentNullException.ThrowIfNull(col);

            Log("Search row from id");

            return await col.FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// set pulled at to manage last date update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public async Task<int> ReplaceLastPulledAtAsync(DateTime lastPulledAt)
        {
            await _db.CreateTableAsync<LastUpdateAtCol>();

            var collection = _db.Table<LastUpdateAtCol>();

            await _db.DeleteAllAsync<LastUpdateAtCol>();

            var colLastPullAt = new LastUpdateAtCol()
            {
                LastUpdatedAt = lastPulledAt
            };
            var bson = await _db.InsertAsync(colLastPullAt);

            Log("Insert row last pulled at");

            return bson;
        }

        /// <summary>
        /// get pulled at to manage last date update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public async Task<DateTime> GetLastPulledAtAsync()
        {
            await _db.CreateTableAsync<LastUpdateAtCol>();

            var col = _db.Table<LastUpdateAtCol>();
           

            var lastPulledAtCollection = await col.FirstOrDefaultAsync();

            if (lastPulledAtCollection != null)
            {
                Log("get row last pulled at");

                return lastPulledAtCollection.LastUpdatedAt;
            }

            Log("first pull server to local database, not exist last pulled");

            return DateTime.MinValue;
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property createdAt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Task<int> InsertSyncAsync<T>(T entity) where T : SchemaSync, new()
        {
            ArgumentNullException.ThrowIfNull(entity);
            AsyncTableQuery<T> col = _db.Table<T>();
            ArgumentNullException.ThrowIfNull(col);

            Log("Insert row");
            entity.CreateAt(_options?.Time);

            return _db.InsertAsync(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property updatedAt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Task<int> UpdateSyncAsync<T>(T entity) where T : SchemaSync, new()
        {
            ArgumentNullException.ThrowIfNull(entity);

            AsyncTableQuery<T> col = _db.Table<T>();

            ArgumentNullException.ThrowIfNull(col);

            Log("update row");
            entity.UpdateAt(_options?.Time);

            return _db.UpdateAsync(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property deletedAt
        /// Its Soft Delete!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Task<int> DeleteSyncAsync<T>(T entity) where T : SchemaSync, new()
        {
            ArgumentNullException.ThrowIfNull(entity);

            AsyncTableQuery<T> col = _db.Table<T>();

            ArgumentNullException.ThrowIfNull(col);

            Log("delete row");
            entity.DeleteAt(_options?.Time);

            return _db.UpdateAsync(entity);
        }

        /// <summary>
        /// Update local database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schemaPush"></param>
        /// <returns></returns>
        public async Task<SchemaPush<T>> PushChangesResultAsync<T>(SchemaPush<T> schemaPush) where T : SchemaSync, new()
        {
            ArgumentNullException.ThrowIfNull(schemaPush);

            if (_db is null)
            {
                throw new PushChangeException("Database not initialized");
            }

            if (!schemaPush.HasChanges)
            {
                Log($"Not Changes");
                return schemaPush;
            }

            AsyncTableQuery<T> col = _db.Table<T>();

            if (col is null)
            {
                Log($"Collection not found");

                throw new PushChangeException("Collection not found");
            }

           await _db.RunInTransactionAsync(async (sqlConnection) =>
              {
                  try
                  {
                      Log($"Start push changes");

                      var lastPulledAt = DateTime.UtcNow.GetDaTimeFromConfig(_options?.Time);

                      var idsRemotedCreated = schemaPush.Changes.Created.Select(s => s.Id);

                      var idsDatabaseLocal = (await col
                          .Where(s => idsRemotedCreated.Contains(s.Id))
                          .ToListAsync())
                          .Select(s => s.Id)
                          .AsEnumerable();

                      var idsNotExistsLocalDatabase = schemaPush.Changes.Created.Where(s => !idsDatabaseLocal.Contains(s.Id));

                      Log($"Start transaction database");
                      var totalInsertId = 0;

                      if (idsNotExistsLocalDatabase.Any())
                      {
                          totalInsertId = await _db.InsertAllAsync(idsNotExistsLocalDatabase);
                      }

                      Log($"Total {totalInsertId} inserted");

                      if (schemaPush.HasUpdated)
                      {
                          foreach (var item in schemaPush.Changes.Updated)
                          {
                              await UpdateSyncAsync(item);
                          }
                      }

                      if (schemaPush.HasDeleted)
                      {
                          List<int>? totalDeleted = [];
                          foreach (var id in schemaPush.Changes.Deleted)
                          {
                              totalDeleted.Add(await _db.DeleteAsync<T>(id));
                          }
                          Log($"Total  rows {totalDeleted.Count} deleted");
                      }

                      //tran.Commit();

                      schemaPush.SetCommitDatabaseOperation(true);

                      Log($"Commit transaction database is {schemaPush.CommitedDatabaseOperation}");
                  }
                  catch (Exception ex)
                  {
                      schemaPush.SetCommitDatabaseOperation(false);
                      Log($"Error Push Changes");
                      //tran.Rollback();
                      Log($"Rollback transaction database");
                      Log(ex.Message);

                      if (ex.InnerException != null)
                          Log(ex.InnerException.Message);
                  }
              });

            return schemaPush;
        }

        public void DumpLogOutput(string title = "log.txt")
        {
            if (_options?.Mode == Mode.DEBUG && _options.SaveLogOnFile)
            {
                using StreamReader r = File.OpenText($"{_options?.PathFile}\\{title}");
                string? line;

                while ((line = r.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Log(object logMessage, string title = "log.txt")
        {
            if (_options?.Mode == Mode.DEBUG)
            {
                if (_options.SaveLogOnFile)
                {
                    using TextWriter w = File.AppendText($"{_options?.PathFile}\\{title}");
                    w.Write("\r\nLog Entry :");
                    w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    w.WriteLine("  :");
                    w.WriteLine($"  :{logMessage}");
                    w.WriteLine("########################################################################");
                }
                else
                {
                    var msg = new StringBuilder();
                    msg.Append("\r\nLog Entry :");
                    msg.AppendLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    msg.AppendLine("  :");
                    msg.AppendLine($"  :{logMessage}");
                    msg.AppendLine("########################################################################");

                    Console.WriteLine(msg.ToString());
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _options = null;
                }

                _disposedValue = true;
            }
        }
    }
}