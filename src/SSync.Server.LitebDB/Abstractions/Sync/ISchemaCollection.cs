﻿using SSync.Server.LitebDB.Engine;
using System.Text.Json.Nodes;

namespace SSync.Server.LitebDB.Abstractions
{
    public interface ISchemaCollection
    {
        Task<List<object>> PullChangesAsync(SSyncParamenter parameter, SSyncOptions? options = null);

        Task<bool> PushChangesAsync(JsonArray changes, SSyncParamenter parameter, SSyncOptions? options = null);
    }
}