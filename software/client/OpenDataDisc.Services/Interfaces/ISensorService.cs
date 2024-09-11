using System.Threading.Tasks;
using OpenDataDisc.Services.Models;

namespace OpenDataDisc.Services.Interfaces
{
    public interface ISensorService
    {
        Task SaveSensorData(SensorData sensorData);
        Task<int> MessagesReceivedInLastNSeconds(int lastNSeconds);
    }
}
