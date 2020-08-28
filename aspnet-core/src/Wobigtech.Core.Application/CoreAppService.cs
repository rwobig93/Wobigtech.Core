using System;
using System.Collections.Generic;
using System.Text;
using Wobigtech.Core.Localization;
using Volo.Abp.Application.Services;

namespace Wobigtech.Core
{
    /* Inherit your application services from this class.
     */
    public abstract class CoreAppService : ApplicationService
    {
        protected CoreAppService()
        {
            LocalizationResource = typeof(CoreResource);
        }
    }
}
