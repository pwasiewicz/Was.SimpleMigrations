namespace Was.SimpleMigrations
{
    using System.Linq;

    public interface IMigrationMetaStorage
    {
        IQueryable<MigrationInfo> Query();
        void New(MigrationInfo migrationInfo);
        void Update(MigrationInfo migrationInfo);
    }
}
