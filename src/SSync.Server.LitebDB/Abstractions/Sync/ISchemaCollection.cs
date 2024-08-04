using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions
{
    public interface ISchemaCollection
    {
        Task<SchemaPullResult<TCollection>> CheckChanges<TCollection, TParamenter>(TParamenter paramenter, SSyncOptions? options = null)
            where TCollection : ISchema
            where TParamenter : SSyncParamenter;

        Task<List<object>> PullChangesAsync(SSyncParamenter parameter, SSyncOptions? options = null);


        //Task<ICollection<SchemaPullResult<TCollection>>> PullChangesAsync<TCollection, TParamenter>(TParamenter paramenter, SSyncOptions? options = null)
        //    where TCollection : ISchema
        //    where TParamenter : SSyncParamenter;

    }
}
