using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSync.Server.LiteDB.PlayGround.Data;
using SSync.Server.LiteDB.PlayGround.Model;

namespace SSync.Server.LiteDB.PlayGround.Endpoints;

public static class CrudEndpoints
{
    public static void AddCrudEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/list", async ([FromServices] TestDbContext cxt) =>
        {
            var usersDb = await cxt.User.ToListAsync();
            var financesDb = await cxt.Finances.ToListAsync();

            return Results.Ok(new { usersDb, financesDb });
        });

        app.MapGet("/create", async ([FromServices] TestDbContext cxt) =>
        {
            var now = DateTime.UtcNow;
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = $" Cotoso {new Random().Next()}",
                CreatedAt = now,
                UpdatedAt = now
            };
            cxt.User.Add(user);

            var finance = new Finance()
            {
                Id = Guid.NewGuid(),
                Price = new Random().Next(),
                CreatedAt = now,
                UpdatedAt = now
            };

            cxt.Finances.Add(finance);

            var res = await cxt.SaveChangesAsync();
            return Results.Ok(res);
        });

        app.MapGet("/user-update/{userId}", async (Guid userId, [FromServices] TestDbContext cxt) =>
        {
            var now = DateTime.UtcNow;

            var user = cxt.User.Find(userId);

            if (user is null)
            {
                return Results.NotFound();
            }

            user.Name = $"Update {new Random().Next()}";

            user.SetUpdatedAt(DateTime.UtcNow);

            cxt.User.Update(user);

            var res = await cxt.SaveChangesAsync();

            return Results.Ok(res);
        });

        app.MapGet("/finance-update/{id}", async (Guid id, [FromServices] TestDbContext cxt) =>
        {
            var now = DateTime.UtcNow;

            var finance = cxt.Finances.Find(id);

            if (finance is null)
            {
                return Results.NotFound();
            }

            finance.Price = new Random().Next();

            finance.SetUpdatedAt(DateTime.UtcNow);

            cxt.Finances.Update(finance);

            var res = await cxt.SaveChangesAsync();

            return Results.Ok(res);
        });

        app.MapGet("/finance-delete/{id}", async (Guid id, TestDbContext cxt) =>
        {
            var now = DateTime.UtcNow;

            var finance = cxt.Finances.Find(id);

            if (finance is null)
            {
                return Results.NotFound();
            }

            finance.SetDeletedAt(DateTime.UtcNow);

            cxt.Finances.Update(finance);

            var res = await cxt.SaveChangesAsync();

            return Results.Ok(res);
        });

        app.MapGet("/user-delete/{id}", async (Guid id, TestDbContext cxt) =>
        {
            var now = DateTime.UtcNow;

            var user = cxt.User.Find(id);

            if (user is null)
            {
                return Results.NotFound();
            }

            user.SetDeletedAt(DateTime.UtcNow);

            cxt.User.Update(user);

            var res = await cxt.SaveChangesAsync();

            return Results.Ok(res);
        });
    }
}