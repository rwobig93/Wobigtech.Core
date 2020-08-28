using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace Wobigtech.Core.HttpApi.Client.ConsoleTestApp
{
    [DependsOn(
        typeof(CoreHttpApiClientModule),
        typeof(AbpHttpClientIdentityModelModule)
        )]
    public class CoreConsoleApiClientModule : AbpModule
    {
        
    }
}
