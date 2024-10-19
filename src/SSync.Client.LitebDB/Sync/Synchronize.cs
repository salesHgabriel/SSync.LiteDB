using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Enums;
using SSync.Client.LitebDB.Exceptions;
using SSync.Client.LitebDB.Extensions;
using SSync.Client.LitebDB.Poco;
using System.Text;

namespace SSync.Client.LitebDB.Sync
{
    public class Synchronize : ISynchronize
    {
        private readonly LiteDatabase _db;
        private readonly SynchronizeOptions? _options;

        public Synchronize(LiteDatabase db, SynchronizeOptions? options = null)
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
        public SchemaPullResult<T> PullChangesResult<T>(long lastPulledAt, string collectionName) where T : SchemaSync
        {
            if (_db is null)
            {
                Log($"Database not initialized");
                throw new PullChangesException("Database not initialized");
            }

            if (lastPulledAt < 0)
            {
                Log($"Range less of zero");

                throw new PullChangesException("Range less of zero");
            }

            Log("Start fetch local changes");

            collectionName ??= typeof(T).Name;
            var timestamp = DateTime.UtcNow.ToUnixTimestamp(_options?.Time);
            var doc = _db.GetCollection<T>(collectionName);

            if (doc is null)
            {
                Log($"Collection not found");

                throw new PullChangesException("Collection not found");
            }

            var createdQuery = doc.Query()
                .Where(d => d.Status == StatusSync.CREATED);

            var updatedQuery = doc.Query()
                .Where(d => d.Status == StatusSync.UPDATED);

            var deletedQuery = doc.Query()
                .Where(d => d.Status == StatusSync.DELETED);

            if (lastPulledAt == 0)
            {
                var createds = createdQuery.ToEnumerable();
                var updateds = updatedQuery.ToEnumerable();
                var deleteds = deletedQuery
                    .Select(d => d.Id)
                    .ToEnumerable();

                Log("Successfully fetch local all changes");

                return new SchemaPullResult<T>(collectionName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
            }

            return new SchemaPullResult<T>(
                collectionName,
                timestamp,
                new SchemaPullResult<T>.Change(

                  created: createdQuery
                                    .Where(d => d.CreatedAt >= lastPulledAt)
                                    .Where(d => !d.DeletedAt.HasValue)
                                    .ToEnumerable(),

                    updated: updatedQuery
                                        .Where(d => d.UpdatedAt >= lastPulledAt)
                                        .Where(d => d.UpdatedAt != d.CreatedAt)
                                        .Where(d => !d.DeletedAt.HasValue)
                                         .ToEnumerable(),

                    deleted: deletedQuery
                                        .Where(d => d.DeletedAt >= lastPulledAt)
                                        .Where(d => d.DeletedAt.HasValue)
                                        .Select(d => d.Id)
                                        .ToEnumerable()
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
        public T FindByIdSync<T>(Guid id, ILiteCollection<T> col) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(col);

            Log("Search row from id");

            return col.FindById(id);
        }

        /// <summary>
        /// Wrapper to use search collection from id
        /// Search collection from by name, if not set search for default name of class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public T FindByIdSync<T>(Guid id, string? colName) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(colName);

            colName ??= typeof(T).Name;

            var col = _db.GetCollection<T>(colName);

            if (col is null) ArgumentNullException.ThrowIfNull(col);

            Log("Search row from id");

            return col.FindById(id);
        }

        /// <summary>
        /// set pulled at to manage last date update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue ReplaceLastPulledAt(long lastPulledAt)
        {
            var collection = _db.GetCollection<LastUpdateAtCol>(typeof(LastUpdateAtCol).Name);
            
            //fix empty page database
            _db.Rebuild();

            collection.DeleteAll();

            var colLastPullAt = new LastUpdateAtCol()
            {
                LastUpdatedAt = (int)lastPulledAt
            };
            var bson = collection.Insert(colLastPullAt);

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
        public long GetLastPulledAt(Time? time)
        {
            time ??= Time.UTC;
            try
            {
                _db.Rebuild();

                var col = _db.GetCollection<LastUpdateAtCol>(typeof(LastUpdateAtCol).Name);
                var lastPulledAtCollection = col.FindAll().FirstOrDefault();

                if (lastPulledAtCollection == null)
                {
                    return DateTime.UtcNow.ToUnixTimestamp(time);
                }

                Log("get row last pulled at");

                return lastPulledAtCollection.LastUpdatedAt;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// this abstraction focus set automatically set date on property createdAt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue InsertSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            ArgumentNullException.ThrowIfNull(col);

            Log("Insert row");
            entity.CreateAt(_options?.Time);

            return col.Insert(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property createdAt
        /// Search collection from by name, if not set search for default name of class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue InsertSync<T>(T entity, string? colName) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            colName ??= typeof(T).Name;

            var col = _db.GetCollection<T>(colName);

            if (col is null) ArgumentNullException.ThrowIfNull(col);

            Log("Insert row");
            entity.CreateAt(_options?.Time);

            return col.Insert(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property updatedAt
        /// Search collection from by name, if not set search for default name of class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue UpdateSync<T>(T entity, string? colName) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            colName ??= typeof(T).Name;

            var col = _db.GetCollection<T>(colName);

            if (col is null) ArgumentNullException.ThrowIfNull(col);

            Log("update row");
            entity.UpdateAt(_options?.Time);

            return col.Update(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property updatedAt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue UpdateSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            ArgumentNullException.ThrowIfNull(col);

            Log("update row");
            entity.UpdateAt(_options?.Time);

            return col.Update(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property deletedAt
        /// Its Soft Delete!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue DeleteSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            ArgumentNullException.ThrowIfNull(col);

            Log("delete row");
            entity.DeleteAt(_options?.Time);

            return col.Update(entity);
        }

        /// <summary>
        /// this abstraction focus set automatically set date on property deletedAt
        /// Its Soft Delete!!
        /// Search collection from by name, if not set search for default name of class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BsonValue DeleteSync<T>(T entity, string? colName) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(entity);

            colName ??= typeof(T).Name;

            var col = _db.GetCollection<T>(colName);

            if (col is null) ArgumentNullException.ThrowIfNull(col);

            Log("delete row");
            entity.DeleteAt(_options?.Time);

            return col.Update(entity);
        }

        /// <summary>
        /// Update local database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schemaPush"></param>
        /// <returns></returns>
        public SchemaPush<T> PushChangesResult<T>(SchemaPush<T> schemaPush) where T : SchemaSync
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

            var col = _db.GetCollection<T>(schemaPush.Collection);

            if (col is null)
            {
                Log($"Collection not found");

                throw new PushChangeException("Collection not found");
            }

            try
            {
                Log($"Start push changes");

                var lastPulledAt = DateTime.Now.ToUnixTimestamp(_options?.Time);
                Log($"Set last pulled At {lastPulledAt}");
                ReplaceLastPulledAt(lastPulledAt);

                var idsRemotedCreated = schemaPush.Changes.Created.Select(s => s.Id);

                var idsDatabaseLocal = col.Query()
                    .Where(s => idsRemotedCreated.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToEnumerable();

                var idsNotExistsLocalDatabase = schemaPush.Changes.Created.Where(s => !idsDatabaseLocal.Contains(s.Id));

                _db.BeginTrans();

                Log($"Start transaction database");

                var totalInsertId = col.InsertBulk(idsNotExistsLocalDatabase);

                Log($"Total {totalInsertId} inserted");

                if (schemaPush.HasUpdated)
                {
                    foreach (var item in schemaPush.Changes.Updated)
                    {
                        UpdateSync(item, schemaPush.Collection);
                    }
                }

                if (schemaPush.HasDeleted)
                {
                    var totalDeleted = col.DeleteMany(t => schemaPush.Changes.Deleted.Contains(t.Id));

                    Log($"Total  rows {totalDeleted} deleted");
                }

                schemaPush.SetCommitDatabaseOperation(_db.Commit());

                Log($"Commit transaction database");
            }
            catch (Exception ex)
            {
                Log($"Error Push Changes");
                _db.Rollback();
                Log($"Rollback transaction database");
                Log(ex.Message);

                if (ex.InnerException != null)
                    Log(ex.InnerException.Message);
            }

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
    }
}