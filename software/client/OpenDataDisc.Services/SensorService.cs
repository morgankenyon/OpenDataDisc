using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public class SensorService : ISensorService
    {
        //put somewhere shareable
        private readonly string connectionString = "Data Source=opendatadisc.db;Version=3;New=True;Compress=True";
        public async Task<int> MessagesReceivedInLastNSeconds(int lastNSeconds, CancellationToken token)
        {
            var date = DateTimeOffset.Now.AddSeconds(-1 * lastNSeconds).ToUnixTimeSeconds();

            try
            {
                using SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                await sqlConn.OpenAsync();

                var command = new SQLiteCommand("SELECT COUNT(*) FROM sensor_data WHERE date > ?", sqlConn);

                command.Parameters.AddWithValue("date", date);

                var result = await command.ExecuteScalarAsync(token);

                await sqlConn.CloseAsync();
                if (int.TryParse(result.ToString(), out var messages))
                {
                    return messages;
                }
            }
            catch (Exception ex)
            {
                //TODO: log something
                throw ex;
            }

            return -1;
        }

        public async Task SaveSensorData(SensorData sensorData, CancellationToken token)
        {
            try
            {
                using SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                await sqlConn.OpenAsync();

                var command = new SQLiteCommand("INSERT INTO sensor_data (date, cycleCount, accX, accY, accZ, gyroX, gyroY, gyroZ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", sqlConn);
                command.Parameters.AddWithValue("date", sensorData.Date);
                command.Parameters.AddWithValue("cycleCount", sensorData.CycleCount);
                command.Parameters.AddWithValue("accX", sensorData.AccX);
                command.Parameters.AddWithValue("accY", sensorData.AccY);
                command.Parameters.AddWithValue("accZ", sensorData.AccZ);
                command.Parameters.AddWithValue("gyroX", sensorData.GyroX);
                command.Parameters.AddWithValue("gyroY", sensorData.GyroY);
                command.Parameters.AddWithValue("gyroZ", sensorData.GyroZ);

                await command.ExecuteNonQueryAsync(token);

                await sqlConn.CloseAsync();
            }
            catch (Exception ex)
            {
                //TODO: log something
                throw ex;
            }
        }
    }
}
