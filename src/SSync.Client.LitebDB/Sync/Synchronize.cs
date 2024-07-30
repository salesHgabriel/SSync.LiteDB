using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Poco;

namespace SSync.Client.LitebDB.Sync
{
    public class Synchronize : ISynchronize
    {
        private readonly LiteDatabase _db;

        public Synchronize(LiteDatabase db)
        {
            _db = db;
        }

        /// <summary>
        /// if lastPulledAt is equal 0, load all rows for default
        /// if documentName is null, get name of class  for default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lastPulledAt"></param>
        /// <param name="documentName"></param>
        /// <returns></returns>
        public SchemaPullResult<T> PullChangesResult<T>(long lastPulledAt, string documentName, DateTime now) where T : BaseSync
        {
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

                return new SchemaPullResult<T>(documentName, timestamp, new SchemaPullResult<T>.Change(createds, updateds, deleteds));
            }

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

        //TODO: Testar com muitos dados
        public SchemaPush<T> PushChangesResult<T>(SchemaPush<T> schemaPush) where T : BaseSync
        {
            ArgumentNullException.ThrowIfNull(schemaPush);

            if (!schemaPush.HasChanges) return schemaPush;


            try
            {

                var col = _db.GetCollection<T>(schemaPush.Document);

                var idsRemotedCreateds = schemaPush.Changes.Created.Select(s => s.Id);

                var idsNotExistsLocalDatabase = schemaPush.Changes.Created.Where(s => !idsRemotedCreateds.Contains(s.Id));

                _db.BeginTrans();


                col.InsertBulk(idsNotExistsLocalDatabase);

                foreach (var item in schemaPush.Changes.Updated)
                {
                    col.Update(item);
                }

                if (schemaPush.HasDeleted)
                {
                    var totalDeleted = col.DeleteMany(t => schemaPush.Changes.Deleted.Contains(t.Id));
                }

                _db.Commit();
                return schemaPush;
            }
            catch (Exception ex)
            {
                //TODO: make custom exception transaction push changes
                _db.Rollback();
            }
            return schemaPush;
        }
    }
}