using Wobigtech.Core.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Wobigtech.Core
{
    [DependsOn(
        typeof(CoreEntityFrameworkCoreTestModule)
        )]
    public class CoreDomainTestModule : AbpModule
    {

    }
}