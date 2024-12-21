using SSync.Server.LitebDB.Abstractions;

namespace SSync.Server.LiteDB.PlayGround.Sync.Dto;

public class FinanceSync : ISchema
{
    public FinanceSync(Guid id) : base(id)
    {
    }

    public double Price { get; set; }
}