using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LiteDB.PlayGround.Sync;

public class PlayParamenter : SSyncParameter
{
    public int Time { get; set; } = new Random().Next(100);
}