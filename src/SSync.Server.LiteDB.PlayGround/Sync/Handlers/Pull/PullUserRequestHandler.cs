using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

namespace SSync.Server.LiteDB.PlayGround.Sync.Handlers.Pull;

public class PullUserRequestHandler : ISSyncPullRequest<UserSync, PlayParamenter>
{
    private readonly TestDbContext _ctx;

    public PullUserRequestHandler(TestDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<UserSync>> QueryAsync(PlayParamenter parameter)
    {
        var users = await _ctx.User.Select(u => new UserSync(u.Id)
        {
            Name = u.Name,
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return users;
    }
}