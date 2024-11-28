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
            Action<IPullExecutionOrderStep>? optionsPullChanges = null,
            Action<IPushExecutionOrderStep>?optionsPushChanges = null) where TContext : DbContext, ISSyncDbContextTransaction
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

            
                services.AddScoped<IPullExecutionOrderStep, PullExecutionOrderBuilder>(sp =>
                {
                    var builder = new PullExecutionOrderBuilder();
                    if (optionsPullChanges is not null)
                        optionsPullChanges(builder);
                    return builder;
                });
            


                         
                services.AddScoped<IPushExecutionOrderStep, PushExecutionOrderBuilder>(sp =>
                    {
                        var builder = new PushExecutionOrderBuilder();
                        if (optionsPushChanges is not null)
                            optionsPushChanges(builder);
                        return builder;
                    });
            
            

            services.AddScoped<ISSyncDbContextTransaction>(provider => provider.GetRequiredService<TContext>());

            return services;
        }
    }
}