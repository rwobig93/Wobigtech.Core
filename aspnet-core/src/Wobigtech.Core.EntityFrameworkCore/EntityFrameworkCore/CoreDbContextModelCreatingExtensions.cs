using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Wobigtech.Core.EntityFrameworkCore
{
    public static class CoreDbContextModelCreatingExtensions
    {
        public static void ConfigureCore(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            /* Configure your own tables/entities inside here */

            //builder.Entity<YourEntity>(b =>
            //{
            //    b.ToTable(CoreConsts.DbTablePrefix + "YourEntities", CoreConsts.DbSchema);
            //    b.ConfigureByConvention(); //auto configure for the base class props
            //    //...
            //});
        }
    }
}