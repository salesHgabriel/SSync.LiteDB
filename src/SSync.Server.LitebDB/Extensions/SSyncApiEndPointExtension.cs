using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Engine;


namespace SSync.Server.LitebDB.Extensions;

public static class SSyncApiEndPointExtension
{
    /// <summary>
    /// Add endpoints pull, push and pull stream (IasyncEnumerable)
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="options"></param>
    /// <param name="version"></param>
    /// <param name="route"></param>
    /// <param name="enablePullChangesStream"></param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapApiEndpointsSync<TParamenterSync>(this IEndpointRouteBuilder endpoints,
        SSyncOptions? options = null,
        string version = "v1",
        string route = "sync")
        where TParamenterSync : SSyncParameter, new()
    {
        var routeGroup = endpoints.MapGroup($"api/{version}/{route}");


        routeGroup.MapGet("/pull",
            async ([AsParameters] TParamenterSync parameter, [FromServices] ISchemaCollection schemaCollection) =>
            {
                var pullChangesRemoter = await schemaCollection.PullChangesAsync(parameter, options);

                return Results.Ok(pullChangesRemoter);
            });

        //fix: minimal api not support two complex object, like [FromBody] JsonArray changes and [AsParameters/FromQuery] TParamenterSync paramenter
        // Create this workaround
        routeGroup.MapPost("/push", async (
            [FromBody] JsonArray changes,
            HttpContext httpContext,
            [FromServices] ISchemaCollection schemaCollection) =>
        {
            var query = httpContext.Request.Query;

            var parameter = new TParamenterSync
            {
                Colletions = query["colletions"].ToArray()!,
                Timestamp = DateTime.TryParse(query["timestamp"], out DateTime timestamp)
                    ? timestamp
                    : DateTime.MinValue
            };

            var isOk = await schemaCollection.PushChangesAsync(changes, parameter, options);

            return Results.Ok(isOk);
        });


        routeGroup.MapGet("/pull-stream",
            ([AsParameters] TParamenterSync parameter, [FromServices] ISchemaCollection schemaCollection) =>
            {
                var pullChangesRemoter = schemaCollection.PullStreamChanges(parameter, options);

                return Results.Ok(pullChangesRemoter);
            });


        return routeGroup;
    }
}