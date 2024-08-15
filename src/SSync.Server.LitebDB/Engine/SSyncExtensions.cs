using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine.Builders;
using SSync.Server.LitebDB.Sync;

namespace SSync.Server.LitebDB.Engine
{
    public static class SSyncExtensions
    {
        public static IServiceCollection AddSSyncSchemaCollection<TContext>(this IServiceCollection services,
            Action<IPullExecutionOrderStep>? optionsPullChanges,
            Action<IPushExecutionOrderStep>? optionsPushChanges) where TContext : DbContext, ISSyncDbContextTransaction
        {
            services
                .Scan(scan => scan
                    .FromApplicationDependencies()
                        .AddClasses(classe => classe.AssignableTo(typeof(ISSyncServices)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime()
                        .AddClasses(classe => classe.AssignableTo(typeof(IInternalISSyncPushRequest<>)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime()
                       .AddClasses(classe => classe.AssignableTo(typeof(ISSyncPullRequest<,>)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime()
                        .AddClasses(classe => classe.AssignableTo(typeof(ISSyncPushRequest<>)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime()
                );

            services.AddScoped<ISchemaCollection, SchemaCollection>();

            //var builder = new ExecutionOrderBuilder();
            //optionsPullChanges(builder);

            //services.AddSingleton(builder);

            if (optionsPullChanges is not null)
            {
                services.AddScoped<IPullExecutionOrderStep, PullExecutionOrderBuilder>(sp =>
                {
                    var builder = new PullExecutionOrderBuilder();
                    optionsPullChanges(builder);
                    return builder;
                });
            }

            if (optionsPushChanges is not null)
            {
                services.AddScoped<IPushExecutionOrderStep, PushExecutionOrderBuilder>(sp =>
                {
                    var builder = new PushExecutionOrderBuilder();
                    optionsPushChanges(builder);
                    return builder;
                });
            }

            services.AddScoped<ISSyncDbContextTransaction>(provider =>
               provider.GetRequiredService<TContext>()
               );

            return services;
        }
    }
}