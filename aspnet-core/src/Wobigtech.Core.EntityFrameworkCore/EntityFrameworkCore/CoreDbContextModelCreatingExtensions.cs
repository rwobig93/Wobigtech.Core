using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

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

            builder.Entity<GameServer.Game>(b =>
            {
                b.ToTable(CoreConsts.DbTablePrefix + "Games", CoreConsts.DbSchema);
                b.ConfigureByConvention();
            });
            builder.Entity<GameServer.GameServer>(b =>
            {
                b.ToTable(CoreConsts.DbTablePrefix + "GameServers", CoreConsts.DbSchema);
                b.ConfigureByConvention();
            });
            builder.Entity<GameServer.Server>(b =>
            {
                b.ToTable(CoreConsts.DbTablePrefix + "Server", CoreConsts.DbSchema);
                b.ConfigureByConvention();
            });
        }
    }
}