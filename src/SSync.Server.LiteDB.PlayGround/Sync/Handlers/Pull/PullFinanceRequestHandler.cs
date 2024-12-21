using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

namespace SSync.Server.LiteDB.PlayGround.Sync.Handlers.Pull;

public class PullFinanceRequestHandler : ISSyncPullRequest<FinanceSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullFinanceRequestHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<FinanceSync>> QueryAsync(PlayParamenter parameter)
    {
        var finances = await _ctx.Finances.Select(u => new FinanceSync(u.Id)
        {
            Price = new Random().Next(100),
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return finances;
    }
}