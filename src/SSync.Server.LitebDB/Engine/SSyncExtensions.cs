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
        public static IServiceCollection AddSSyncSchemaCollection(this IServiceCollection services,
            Action<IExecutionOrderStep> optionsPullChanges,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services
                .Scan(scan => scan
                    .FromApplicationDependencies()
                        .AddClasses(classe => classe.AssignableTo(typeof(ISSyncServices)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime()
                        .AddClasses(classe => classe.AssignableTo(typeof(ISSyncPullRequest<,>)))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());

            services.AddScoped<ISchemaCollection, SchemaCollection>();


            var builder = new ExecutionOrderBuilder();
            optionsPullChanges(builder);

            services.AddSingleton(builder);

            //services.Add(new ServiceDescriptor(typeof(WatermelonSchemaConfiguration<TSchemaCollection>), provider => new WatermelonSchemaConfiguration<TSchemaCollection>(provider.GetRequiredService<ISSyncServices<TSchemaCollection>>()), serviceLifetime));



            return services;
        }
    }
}