using System.Threading.Tasks;

namespace Wobigtech.Core.Data
{
    public interface ICoreDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
