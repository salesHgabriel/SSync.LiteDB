using SSync.Server.LitebDB.Abstractions;

namespace SSync.Server.LiteDB.PlayGround.Model;

public class User : ISSyncEntityRoot
{
    public string? Name { get; set; }

}