using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Exceptions;
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
        public async Task<SchemaPullResult<T>> PullChangesResultAsync<T>(long lastPulledAt, string documentName, DateTime now) where T : BaseSync
        {
            try
            {
                await LogAsync("Start fetch local changes");

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

                    await LogAsync("Succefull fetch local all changes");

                    return new SchemaPullResult<T>(documentName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
                }

                await LogAsync($"Succefull fetch local changes from last pulled At {lastPulledAt}");

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
                await LogAsync($"Ops Erro fetch changes", consoleColor: ConsoleColor.Red);
                await LogAsync(ex);
                await LogAsync(ex.Message);
                await LogAsync(ex.InnerException?.Message ?? string.Empty);
            }

            return new SchemaPullResult<T>();
        }

        public async Task<SchemaPush<T>> PushChangesResultAsync<T>(SchemaPush<T> schemaPush) where T : BaseSync
        {
            ArgumentNullException.ThrowIfNull(schemaPush);

            if (!schemaPush.HasChanges) return schemaPush;

            try
            {
                await LogAsync($"Start push changes");

                var col = _db.GetCollection<T>(schemaPush.Document);

                var idsRemotedCreateds = schemaPush.Changes.Created.Select(s => s.Id);

                var idsNotExistsLocalDatabase = schemaPush.Changes.Created.Where(s => !idsRemotedCreateds.Contains(s.Id));

                _db.BeginTrans();

                await LogAsync($"Start transaction database");

                var totalInsertId = col.InsertBulk(idsNotExistsLocalDatabase);

                await LogAsync($"Total {totalInsertId} inserted");

                foreach (var item in schemaPush.Changes.Updated)
                {
                    col.Update(item);
                }

                if (schemaPush.HasDeleted)
                {
                    var totalDeleted = col.DeleteMany(t => schemaPush.Changes.Deleted.Contains(t.Id));
                    await LogAsync($"Total {totalDeleted} deleteds");
                }

                _db.Commit();

                await LogAsync($"Commit transaction database");

                return schemaPush;
            }
            catch (PushChangeException ex)
            {
                _db.Rollback();

                await LogAsync($"Ops Erro push changes", consoleColor: ConsoleColor.Red);
                await LogAsync(ex);
                await LogAsync(ex.Message);
                await LogAsync(ex.InnerException?.Message ?? string.Empty);
            }
            return schemaPush;
        }

        public async Task LogAsync(object logMessage, string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Yellow)
        {
            if (_options?.Mode == Mode.DEBUG)
            {
                var msg = new StringBuilder();

                msg.AppendLine("\r\nLog Entry :)");
                msg.AppendLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                msg.AppendLine("  :");
                msg.AppendLine($"  :{logMessage}");
                msg.AppendLine("########################################################################");

                if (_options.SaveLogOnFile)
                {
                    await File.AppendAllTextAsync($"{_options?.ConnectionString}\\{title}", msg.ToString());
                }
                else
                {
                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine(msg.ToString());
                }
            }
        }

        public void DumpLogOutput(string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Yellow)
        {
            if (_options?.Mode == Mode.DEBUG && _options.SaveLogOnFile)
            {
                using StreamReader r = File.OpenText($"{_options?.ConnectionString}\\{title}");
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