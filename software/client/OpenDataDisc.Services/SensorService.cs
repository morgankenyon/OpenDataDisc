using OpenDataDisc.Services.Interfaces;
using System;
using System.Data.SQLite;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    public static class SensorService //: ISensorService
    {
        public static Channel<SensorData> SensorChannel = Channel.CreateUnbounded<SensorData>();
        //public static async Task SaveSensorData(SensorData sensorData)
        //{
        //    //put somewhere shareable
        //    SQLiteConnection sqlConn = new SQLiteConnection("Data Source=opendatadisc.db;Version=3;New=True;Compress=True");

        //    sqlConn.Open();

        //    var command = new SQLiteCommand("INSERT INTO sensor_data (type, date, val1, val2, val3) VALUES (?, ?, ?, ?, ?)", sqlConn);
        //    command.Parameters.Add(sensorData.Type);
        //    command.Parameters.Add(sensorData.Date);
        //    command.Parameters.Add(sensorData.Val1);
        //    command.Parameters.Add(sensorData.Val2);
        //    command.Parameters.Add(sensorData.Val3);

        //    try
        //    {
        //        await command.ExecuteNonQueryAsync();
        //    }
        //    catch (Exception ex) 
        //    {
        //        throw ex;
        //    }
        //}


    }
}
