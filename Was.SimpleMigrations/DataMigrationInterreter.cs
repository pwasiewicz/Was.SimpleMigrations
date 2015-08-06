namespace Was.SimpleMigrations
{
    using EventBus.Extensions;
    using Helper;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal class DataMigrationInterpreter : IDisposable
    {
        public const int NoMigrationsFound = -1;
        public const string MigrationMethodNamePattern = @"^UpdateFrom[\d]+$";
        public const string MigrationMethdNameExtractVersion = @"^UpdateFrom([\d]+)$";

        private readonly IDataMigration dataMigrationInvoker;
        private readonly IMigrationMetaStorage storage;

        public DataMigrationInterpreter(IDataMigration dataMigrationInvoker, IMigrationMetaStorage storage)
        {
            this.dataMigrationInvoker = dataMigrationInvoker;
            this.storage = storage;
        }

        public void Start()
        {
            var migrationTypes =
                this.storage.Query()
                    .OrderBy(migrType => migrType.MigrationName)
                    .ToList()
                    .ToDictionary(migration => migration.MigrationName);

            foreach (var migration in this.dataMigrationInvoker.GetUnderlyingInstances().Cast<IDataMigration>())
            {
                var migrationIdentifier = migration.GetType().FullName;

                this.PerformMigration(
                    migration,
                    migrationTypes.ValueOrDefault(
                        migrationIdentifier, null));
            }
        }

        private void PerformMigration(IDataMigration migration, MigrationInfo migrationEntity)
        {
            var migrationType = migration.GetType();

            bool newMigration = false;

            if (migrationEntity == null)
            {
                newMigration = true;
                migrationEntity = new MigrationInfo { MigrationName = migration.GetType().FullName, Version = -1 };
            }

            try
            {
                migrationEntity.Version = PerformMigration(migration, migrationType,
                                                           migrationEntity.Version + 1);

                if (migrationEntity.Version == NoMigrationsFound)
                {
                    return;
                }

                if (newMigration)
                {
                    this.storage.New(migrationEntity);
                }
                else
                {
                    this.storage.Update(migrationEntity);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static int PerformMigration(IDataMigration migration, Type migrationType, int nextVersion)
        {
            var migrationMethodVersions =
                migrationType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                             .Where(IsMigrationMethod)
                             .ToDictionary(ExtractAvailableMethodVersion, method => method);

            var allVersionSorted = migrationMethodVersions.Keys.OrderBy(version => version).ToList();

            if (!migrationMethodVersions.Any())
            {
                return NoMigrationsFound;
            }

            var migrationIndex = allVersionSorted.BinarySearch(nextVersion);
            if (migrationIndex < 0)
            {
                return NoMigrationsFound;
            }

            for (var i = migrationIndex; i < allVersionSorted.Count; i++)
            {
                var version = allVersionSorted[i];

                try
                {
                    var methodInfo = migrationMethodVersions[version];
                    if (methodInfo.GetParameters().Any())
                    {
                        throw new InvalidOperationException("Migration method must be parameterless.");
                    }

                    methodInfo.Invoke(migration, new object[0]);
                }
                catch (Exception ex)
                {
                    throw new InvokingMigrationException(version, migrationType.FullName, ex);
                }
            }

            return allVersionSorted.Last();
        }

        private static int ExtractAvailableMethodVersion(MethodInfo methodInfo)
        {
            var regex = new Regex(MigrationMethdNameExtractVersion);
            var mather = regex.Match(methodInfo.Name);

            return Convert.ToInt32(mather.Groups[1].Value);
        }

        private static bool IsMigrationMethod(MethodInfo methodInfo)
        {
            var methodName = methodInfo.Name;
            return Regex.IsMatch(methodName, MigrationMethodNamePattern);
        }

        public void Dispose()
        {
        }
    }
}
