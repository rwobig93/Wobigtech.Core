using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wobigtech.Core
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication<CoreHttpApiHostModule>();
        }

        // , IWebHostEnvironment env, ILoggerFactory loggerFactory
        public void Configure(IApplicationBuilder app)
        {
            app.InitializeApplication();
        }
    }
}
