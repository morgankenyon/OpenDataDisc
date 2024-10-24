using OpenDataDisc.Services.Models;
using System.Threading.Tasks;

namespace OpenDataDisc.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<DiscConfigurationData?> SearchForDeviceConfiguration(string deviceId);
    }
}
