using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Exceptions;
using SSync.Client.LitebDB.Abstractions.Sync;
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
        /// if documentName is null, get name of class  for default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lastPulledAt"></param>
        /// <param name="documentName"></param>
        /// <returns></returns>
        public SchemaPullResult<T> PullChangesResultAsync<T>(long lastPulledAt, string documentName, DateTime now) where T : BaseSync
        {
            try
            {
                Log("Start fetch local changes");

                documentName ??= typeof(T).Name;
                var timestamp = now.Ticks;
                var doc = _db.GetCollection<T>(documentName);

                var createdQuery = doc.Query().Where(d => d.Status == StatusSync.CREATED);
                var updatedQuery = doc.Query().Where(d => d.Status == StatusSync.UPDATED);
                var deletedQuery = doc.Query().Where(d => d.Status == StatusSync.DELETED);

                if (lastPulledAt == 0)
                {
                    var createds = createdQuery.ToEnumerable();
                    var updateds = updatedQuery.ToEnumerable();
                    var deleteds = deletedQuery
                        .Select(d => d.Id)
                        .ToEnumerable();

                    Log("Succefull fetch local all changes");

                    return new SchemaPullResult<T>(documentName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
                }

                Log($"Succefull fetch local changes from last pulled At {lastPulledAt}");

                return new SchemaPullResult<T>(
                    documentName,
                    timestamp,
                    new SchemaPullResult<T>.Change(
                        createdQuery.Where(d => d.CreatedAt > lastPulledAt).ToEnumerable(),
                        updatedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.UpdatedAt > lastPulledAt).ToEnumerable(),
                        deletedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.DeletedAt > lastPulledAt).Select(d => d.Id).ToEnumerable()
                        )
                    );
            }
            catch (PullChangesException ex)
            {
                Log($"Ops Erro fetch changes", consoleColor: ConsoleColor.Red);
                Log(ex);
                Log(ex.Message);
                Log(ex.InnerException?.Message ?? string.Empty);
            }

            return new SchemaPullResult<T>();
        }

        /// <summary>
        /// Update local database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schemaPush"></param>
        /// <returns></returns>
        public SchemaPush<T> PushChangesResultAsync<T>(SchemaPush<T> schemaPush) where T : BaseSync
        {
            ArgumentNullException.ThrowIfNull(schemaPush);

            if (!schemaPush.HasChanges) return schemaPush;

            try
            {
                Log($"Start push changes");

                var col = _db.GetCollection<T>(schemaPush.Document);

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

                _db.Commit();

                Log($"Commit transaction database");

                return schemaPush;
            }
            catch (PushChangeException ex)
            {
                _db.Rollback();

                Log($"Ops Erro push changes", consoleColor: ConsoleColor.Red);
                Log(ex);
                Log(ex.Message);
                Log(ex.InnerException?.Message ?? string.Empty);
            }
            return schemaPush;
        }

        public void Log(object logMessage, string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Yellow)
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

                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine(msg.ToString());
                }
            }
        }

        public void DumpLogOutput(string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Yellow)
        {
            if (_options?.Mode == Mode.DEBUG && _options.SaveLogOnFile)
            {
                using StreamReader r = File.OpenText($"{_options?.PathFile}\\{title}");
                string? line;

                Console.ForegroundColor = consoleColor;

                while ((line = r.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
}