# Was.SimpleMigrations

Simple module for perfoming migrations.

Available as nuget package.

### Usage

#### Running
```c#
MigrationResolver
   .RunMigrations(migrationBuilder =>
            {
                 // Required implementation of IMigrationMetaStorage
                 migrationBld.RegisterType<MigrationStorage>().As<IMigrationMetaStorage>();
                 
                 // registration of other dependecies that should be available in data migration
                 migrationBld.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
                 migrationBld.Register(typeof(ApplicationDbContext)).As(typeof(DbContext));
            }, Assembly.GetExecutingAssembly());
```

#### IMigrationMetaStorage implementation
Implementation of IMigrationMetaStorage is neccessary to setup a place, where info about pending / further / done migrations is stored.

Sample implementation with Repository pattern:
```c#

internal class MigrationStorage : IMigrationMetaStorage
{
    private readonly IRepository<DataMigrationVersion> dataRepository;
    
    // dependency registered in RunMigrations method, see Running section
    public MigrationStorage(IRepository<DataMigrationVersion> dataRepository)
    {
         this.dataRepository = dataRepository;
    }

    public IQueryable<MigrationInfo> Query()
    {
        return this.dataRepository.AsQuerable().Select(m => new MigrationInfo
                                                            {
                                                                 MigrationName = m.TypeName,
                                                                 Version = m.Version
                                                             });
    }

    public void New(MigrationInfo migrationInfo)
    {
        this.dataRepository.Create(new DataMigrationVersion
                                    {
                                        TypeName = migrationInfo.MigrationName,
                                        Version = migrationInfo.Version
                                    });

        this.dataRepository.Save();
    }

    public void Update(MigrationInfo migrationInfo)
    {
        var m = this.dataRepository.AsQuerable().Single(dm => dm.TypeName == migrationInfo.MigrationName);
        m.Version = migrationInfo.Version;
        this.dataRepository.Update(m);
        this.dataRepository.Save();
    }
}
```

#### Sample migration
```c#
namespace SomeMigrations
{
  public class UserMigrations : IDataMigration
  {
      prvate readonly DbContext dbContext;
       
      public UserMigrations(DbContext ctx) 
      {
          this.dbContext = ctx;
      }
      
      public void UpdateFrom0()
      {
           // some stuff
      }

      public void UpdateFrom1 () { }
  }
}
```
