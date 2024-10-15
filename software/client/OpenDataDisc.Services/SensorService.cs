using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public class SensorService : ISensorService
    {
        //put somewhere shareable
        private readonly string connectionString = "Data Source=opendatadisc.db;Version=3;New=True;Compress=True";
        public async Task<int> MessagesReceivedInLastNSeconds(int lastNSeconds)
        {
            var date = DateTimeOffset.Now.AddSeconds(-1 * lastNSeconds).ToUnixTimeSeconds();

            try
            {
                SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                sqlConn.Open();

                var command = new SQLiteCommand("SELECT COUNT(*) FROM sensor_data WHERE date > ?", sqlConn);

                command.Parameters.AddWithValue("date", date);

                var result = await command.ExecuteScalarAsync();

                if (int.TryParse(result.ToString(), out var messages))
                {
                    return messages;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return -1;
        }

        public async Task SaveSensorData(SensorData sensorData)
        {
            try
            {
                SQLiteConnection sqlConn = new SQLiteConnection(connectionString);

                sqlConn.Open();

                var command = new SQLiteCommand("INSERT INTO sensor_data (date, cycleCount, accX, accY, accZ, gyroX, gyroY, gyroZ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", sqlConn);
                command.Parameters.AddWithValue("date", sensorData.Date);
                command.Parameters.AddWithValue("cycleCount", sensorData.CycleCount);
                command.Parameters.AddWithValue("accX", sensorData.AccX);
                command.Parameters.AddWithValue("accY", sensorData.AccY);
                command.Parameters.AddWithValue("accZ", sensorData.AccZ);
                command.Parameters.AddWithValue("gyroX", sensorData.GyroX);
                command.Parameters.AddWithValue("gyroY", sensorData.GyroY);
                command.Parameters.AddWithValue("gyroZ", sensorData.GyroZ);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
