using Microsoft.EntityFrameworkCore;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Model;
using SSync.Server.LiteDB.PlayGround.Sync.Dto;

public class PushUserRequestHandler : ISSyncPushRequest<UserSync>
{
    private readonly TestDbContext _db;

    public PushUserRequestHandler(TestDbContext db) => _db = db;

    public async Task<UserSync?> FindByIdAsync(Guid id)
    {
        return await _db.User.Where(u => u.Id == id)
            .Select(us => new UserSync(id)
            {
                Name = us.Name,
                CreatedAt = us.CreatedAt,
                DeletedAt = us.DeletedAt,
                UpdatedAt = us.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CreateAsync(UserSync schema)
    {
        var us = new User()
        {
            Id = schema.Id,
            Name = schema.Name,
            CreatedAt = schema.CreatedAt,
            DeletedAt = schema.DeletedAt,
            UpdatedAt = schema.UpdatedAt
        };

        await _db.User.AddAsync(us);

        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(UserSync schema)
    {
        var us = await _db.User.FindAsync(schema.Id);

        us!.UpdatedAt = DateTime.Now;

        us.Name = schema.Name;

        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(UserSync schema)
    {
        var us = await _db.User.FindAsync(schema.Id);

        us!.DeletedAt = DateTime.Now;

        return await _db.SaveChangesAsync() > 0;
    }
}