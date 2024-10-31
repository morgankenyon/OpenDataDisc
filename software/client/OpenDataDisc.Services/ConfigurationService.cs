using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public class ConfigurationService : IConfigurationService
    {
        //put somewhere shareable
        private readonly string connectionString = "Data Source=opendatadisc.db;Version=3;New=True;Compress=True";

        public async Task SaveDeviceConfiguration(DiscConfigurationData data, CancellationToken token)
        {
            try
            {
                using SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                await sqlConn.OpenAsync();

                var command = new SQLiteCommand("INSERT INTO disc_configuration (deviceId, date, accXOffset, accYOffset, accZOffset, gyroXOffset, gyroYOffset, gyroZOffset) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", sqlConn);
                command.Parameters.AddWithValue("deviceId", data.DeviceId);
                command.Parameters.AddWithValue("date", data.Date);
                command.Parameters.AddWithValue("accXOffset", data.AccXOffset);
                command.Parameters.AddWithValue("accYOffset", data.AccYOffset);
                command.Parameters.AddWithValue("accZOffset", data.AccZOffset);
                command.Parameters.AddWithValue("gyroXOffset", data.GyroXOffset);
                command.Parameters.AddWithValue("gyroYOffset", data.GyroYOffset);
                command.Parameters.AddWithValue("gyroZOffset", data.GyroZOffset);

                await command.ExecuteNonQueryAsync(token);

                await sqlConn.CloseAsync();
            }
            catch (Exception ex)
            {
                //TODO: log something
                throw ex;
            }
        }

        public async Task<DiscConfigurationData?> SearchForDeviceConfiguration(string deviceId, CancellationToken token)
        {
            try
            {
                using SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                await sqlConn.OpenAsync();

                using var command = new SQLiteCommand("SELECT deviceId, date, accXOffset, accYOffset, accZOffset, gyroXOffset, gyroYOffset, gyroZOffset FROM disc_configuration WHERE deviceId = ?", sqlConn);

                command.Parameters.AddWithValue("deviceId", deviceId);

                using var reader = await command.ExecuteReaderAsync();

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

                await sqlConn.CloseAsync();

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
