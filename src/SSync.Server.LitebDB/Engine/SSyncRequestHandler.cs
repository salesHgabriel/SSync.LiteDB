﻿using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Enums;
using SSync.Server.LitebDB.Exceptions;
using SSync.Server.LitebDB.Extensions;

namespace SSync.Server.LitebDB.Engine
{
    public class SSyncRequestHandler<TSchema> : IInternalISSyncPushRequest<TSchema> where TSchema : ISchema
    {
        private readonly ISSyncPushRequest<TSchema> _pushRequest;

        public SSyncRequestHandler(ISSyncPushRequest<TSchema> pushRequest)
        {
            _pushRequest = pushRequest;
        }

        public async Task<bool> UpsertAsync(TSchema schema, DateTime lastPulledAt, Time? time = Time.UTC)
        {
            time ??= Time.UTC;
            var existingSchema = await _pushRequest.FindByIdAsync(schema.Id);

            if (existingSchema is not null)
            {
                if (existingSchema.DeletedAt.HasValue && existingSchema.DeletedAt.Value > DateTime.MinValue)
                    throw new SSyncLiteDBExcepetion(
                        $"Entity {schema.GetType().Name} with key {schema.Id} is already deleted.");
                if (existingSchema.UpdatedAt >= lastPulledAt)
                    throw new SSyncLiteDBExcepetion(
                        $"Entity {schema.GetType().Name} with key {schema.Id} is already updated.");

                return await _pushRequest.UpdateAsync(schema);
            }

            return await _pushRequest.CreateAsync(schema);
        }

        public async Task<bool> DeleteAsync(Guid id, DateTime lastPulledAt, Time? time = Time.UTC)
        {
            time ??= Time.UTC;
            var existingSchema = await _pushRequest.FindByIdAsync(id);

            if (existingSchema is not null)
            {
                if (existingSchema.UpdatedAt >= lastPulledAt)
                    throw new SSyncLiteDBExcepetion(
                        $"Entity {existingSchema.GetType().Name} with key {existingSchema.Id} is already updated.");

                return await _pushRequest.DeleteAsync(existingSchema);
            }

            return true;
        }
    }
}