namespace Was.SimpleMigrations
{
    using Autofac;
    using System;

    public static class MigrationResolver
    {
        public static void RunMigrations(Action<ContainerBuilder> dependenciesRegistrator)
        {
            var migrationBuilder = new ContainerBuilder();

            migrationBuilder.RegisterType<DataMigrationInterpreter>().AsSelf();
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
