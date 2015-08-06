namespace Was.SimpleMigrations
{
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using System;
    using EventBus.Autofac;

    public static class MigrationResolver
    {
        public static void RunMigrations(Action<ContainerBuilder> dependenciesRegistrator, params Assembly[] assemblies)
        {
            var migrationBuilder = new ContainerBuilder();

            migrationBuilder.RegisterType<DataMigrationInterpreter>().AsSelf();

            var totalAssemblies = assemblies.Concat(new[] { Assembly.GetAssembly(typeof(IDataMigration)) }).ToArray();
            migrationBuilder.RegisterModule(new EventBusModule(totalAssemblies));

            dependenciesRegistrator(migrationBuilder);

            using (var scope = migrationBuilder.Build().BeginLifetimeScope())
            {
                if (!scope.IsRegistered(typeof(IMigrationMetaStorage)))
                {
                    throw new InvalidOperationException("Implementation of IMigrationMetaStorage is not registered.");
                }

                using (var migrationsInterpreter = scope.Resolve<DataMigrationInterpreter>())
                {
                    migrationsInterpreter.Start();
                }
            }
        }
    }
}
