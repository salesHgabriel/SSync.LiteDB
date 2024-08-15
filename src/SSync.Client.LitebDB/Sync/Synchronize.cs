using LiteDB;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Poco;
using SSync.Shared.ClientServer.LitebDB.Enums;
using SSync.Shared.ClientServer.LitebDB.Exceptions;
using SSync.Shared.ClientServer.LitebDB.Extensions;
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
        public SchemaPullResult<T> PullChangesResult<T>(long lastPulledAt, string collectionName, DateTime now) where T : SchemaSync
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
            var timestamp = now.ToUnixTimestamp();
            var doc = _db.GetCollection<T>(collectionName);

            if (doc is null)
            {
                Log($"Collection not found");

                throw new PullChangesException("Collection not found");
            }

            var createdQuery = doc.Query()
                .Where(d => d.CreatedAt > 0)
                .Where(d => d.Status == StatusSync.CREATED);

            var updatedQuery = doc.Query()
                .Where(d => d.UpdatedAt > 0)
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

                Log("Succefully fetch local all changes");

                return new SchemaPullResult<T>(collectionName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
            }

            Log($"Succefully fetch local changes from last pulled At {lastPulledAt}");

            return new SchemaPullResult<T>(
                collectionName,
                timestamp,
                new SchemaPullResult<T>.Change(
                    createdQuery.Where(d => d.CreatedAt > lastPulledAt).ToEnumerable(),
                    updatedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.UpdatedAt > lastPulledAt).ToEnumerable(),
                    deletedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.DeletedAt > lastPulledAt).Select(d => d.Id).ToEnumerable()
                    )
                );
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
            entity.CreateAt();

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
            entity.CreateAt();

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
            entity.UpdateAt();

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
            entity.UpdateAt();

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
            entity.DeleteAt();

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
            entity.DeleteAt();

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

            if (!schemaPush.HasChanges) return schemaPush;

            Log($"Start push changes");

            var col = _db.GetCollection<T>(schemaPush.Collection);

            if (col is null)
            {
                Log($"Collection not found");

                throw new PushChangeException("Collection not found");
            }

            var idsRemotedCreateds = schemaPush.Changes.Created.Select(s => s.Id);

            var idsNotExistsLocalDatabase = schemaPush.Changes.Created.Where(s => !idsRemotedCreateds.Contains(s.Id));

            _db.BeginTrans();

            Log($"Start transaction database");

            var totalInsertId = col.InsertBulk(idsNotExistsLocalDatabase);

            Log($"Total {totalInsertId} inserted");

            foreach (var item in schemaPush.Changes.Updated)
            {
                col.Update(item);
            }

            if (schemaPush.HasDeleted)
            {
                var totalDeleted = col.DeleteMany(t => schemaPush.Changes.Deleted.Contains(t.Id));
                Log($"Total {totalDeleted} deleteds");
            }

            var succeced = _db.Commit();

            schemaPush.SetCommitDatabaseOperation(succeced);

            Log($"Commit transaction database");

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