namespace Was.SimpleMigrations
{
    using System;

    [Serializable]
    public class InvokingMigrationException : Exception
    {
        public int MigrationVersion { get; set; }
        public string MigrationType { get; set; }

        public InvokingMigrationException(int migrationVersion, string migrationType, Exception innerException)
            : base("Error while invoking migration.", innerException)
        {
            this.MigrationVersion = migrationVersion;
            this.MigrationType = migrationType;
        }
    }
}
