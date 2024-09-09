using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public class SensorService : ISensorService
    {
        public async Task SaveSensorData(SensorData sensorData)
        {
            try
            {
                //put somewhere shareable
                SQLiteConnection sqlConn = new SQLiteConnection("Data Source=opendatadisc.db;Version=3;New=True;Compress=True");

                sqlConn.Open();

                var command = new SQLiteCommand("INSERT INTO sensor_data (type, date, val1, val2, val3) VALUES (?, ?, ?, ?, ?)", sqlConn);
                command.Parameters.AddWithValue("type", sensorData.Type.ToString());
                command.Parameters.AddWithValue("date", sensorData.Date);
                command.Parameters.AddWithValue("val1", sensorData.Val1);
                command.Parameters.AddWithValue("val2", sensorData.Val2);
                command.Parameters.AddWithValue("val3", sensorData.Val3);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
