using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Wobigtech.Core.EntityFrameworkCore
{
    [DependsOn(
        typeof(CoreEntityFrameworkCoreModule)
        )]
    public class CoreEntityFrameworkCoreDbMigrationsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<CoreMigrationsDbContext>();
        }
    }
}
