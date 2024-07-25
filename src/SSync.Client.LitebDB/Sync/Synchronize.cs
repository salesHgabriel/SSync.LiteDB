using LiteDB;
using SSync.Client.LitebDB.Abstractions;

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
        public SchemaResult<T> PullChangesResult<T>(long lastPulledAt, string documentName, DateTime now) where T : BaseSync
        {
            documentName ??= typeof(T).Name;
            var timestamp =  now.Ticks;
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

                return new SchemaResult<T>(documentName, timestamp, new SchemaResult<T>.Change(createds, updateds, deleteds));
            }

            return new SchemaResult<T>(
                documentName,
                timestamp,
                new SchemaResult<T>.Change(
                    createdQuery.Where(d => d.CreatedAt > lastPulledAt).ToEnumerable(),
                    updatedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.UpdatedAt > lastPulledAt).ToEnumerable(),
                    deletedQuery.Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.DeletedAt > lastPulledAt).Select(d => d.Id).ToEnumerable()
                    )
                );
        }



        //TODO: MAKE METHOD
        //public bool PushChangesResult<T>(long lastPulledAt, SchemaResult<T> changes, string documentName) where T : BaseSync
        //{
        //    _db.BeginTrans();
        //    documentName ??= typeof(T).Name;
        //    try
        //    {
        //        foreach (var createds in changes.Changes.Created)
        //        {
        //            var doc = _db.GetCollection<T>(nameof(T));
        //            //doc.UpdateAt()
        //            //doc.Insert(createds);
        //        }

        //        foreach (var updateds in changes.Changes.Updated)
        //        {
        //            var doc = _db.GetCollection<T>(nameof(T));
        //            //doc.Update(updateds);
        //        }

        //        foreach (var deleteds in changes.Changes.Deleted)
        //        {
        //            var doc = _db.GetCollection<T>(nameof(T));
        //            //DeleteSync<T>(deleteds, documentName);
        //        }

        //        return _db.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        return _db.Rollback();
        //    }
        //}
    }
}
