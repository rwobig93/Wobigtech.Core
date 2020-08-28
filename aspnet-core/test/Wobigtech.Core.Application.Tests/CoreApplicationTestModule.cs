using Volo.Abp.Modularity;

namespace Wobigtech.Core
{
    [DependsOn(
        typeof(CoreApplicationModule),
        typeof(CoreDomainTestModule)
        )]
    public class CoreApplicationTestModule : AbpModule
    {

    }
}