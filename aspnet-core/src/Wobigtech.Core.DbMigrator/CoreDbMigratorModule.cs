using Wobigtech.Core.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Wobigtech.Core.DbMigrator
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(CoreEntityFrameworkCoreDbMigrationsModule),
        typeof(CoreApplicationContractsModule)
        )]
    public class CoreDbMigratorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
