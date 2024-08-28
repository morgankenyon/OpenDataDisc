using System.Threading.Tasks;

namespace OpenDataDisc.Services.Interfaces
{
    public interface IDataSchemaService
    {
        Task MigrateSchemaToLatest();
    }
}
