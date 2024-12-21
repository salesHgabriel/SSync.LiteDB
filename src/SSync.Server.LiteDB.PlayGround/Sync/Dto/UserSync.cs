using SSync.Server.LitebDB.Abstractions;

namespace SSync.Server.LiteDB.PlayGround.Sync.Dto;

public class UserSync : ISchema
{
    public UserSync(Guid id) : base(id)
    {
    }

    public string? Name { get; set; }
}