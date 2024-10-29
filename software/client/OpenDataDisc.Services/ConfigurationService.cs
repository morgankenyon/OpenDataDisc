using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public class ConfigurationService : IConfigurationService
    {
        //put somewhere shareable
        private readonly string connectionString = "Data Source=opendatadisc.db;Version=3;New=True;Compress=True";

        public Task<bool> SaveDeviceConfiguration(DiscConfigurationData data)
        {
            return Task.FromResult(true);
        }

        public async Task<DiscConfigurationData?> SearchForDeviceConfiguration(string deviceId)
        {
            try
            {
                SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                sqlConn.Open();

                var command = new SQLiteCommand("SELECT deviceId, date, accXOffset, accYOffset, accZOffset, gyroXOffset, gyroYOffset, gyroZOffset FROM disc_configuration WHERE deviceId = ?", sqlConn);

                command.Parameters.AddWithValue("deviceId", deviceId);

                var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    return null;
                }

                reader.Read();

                var dId = reader.GetString(0);
                var date = reader.GetInt64(1);
                var accXOffset = reader.GetDouble(2);
                var accYOffset = reader.GetDouble(3);
                var accZOffset = reader.GetDouble(4);
                var gyroXOffset = reader.GetDouble(5);
                var gyroYOffset = reader.GetDouble(6);
                var gyroZOffset = reader.GetDouble(7);

                return new DiscConfigurationData(
                    dId,
                    date,
                    accXOffset,
                    accYOffset,
                    accZOffset,
                    gyroXOffset,
                    gyroYOffset,
                    gyroZOffset);
            }
            catch (Exception ex)
            {
                //log at some point
                throw ex;
            }
        }
    }
}
