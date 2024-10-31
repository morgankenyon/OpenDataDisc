using OpenDataDisc.Services.Interfaces;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    internal class DataSchemaService : IDataSchemaService
    {
        private readonly Migration _latestMigration = Migration.DiscConfiguration;
        public async Task MigrateSchemaToLatest()
        {
            try
            {
                //TODO: move this db connection somewhere centralized when database operations are more widely used
                SQLiteConnection sqlConn = new SQLiteConnection("Data Source=opendatadisc.db;Version=3;New=True;Compress=True");

                sqlConn.Open();

                var currentMigration = await CalculateLatestMigration(sqlConn);

                if (currentMigration < _latestMigration)
                {
                    await MigrateSchema(currentMigration, sqlConn);
                }
            }
            catch (Exception ex)
            {
                //TODO: add proper error handling, display message to user???
            }
        }

        private async Task<Migration> CalculateLatestMigration(SQLiteConnection connection)
        {
            SQLiteCommand command = connection.CreateCommand();
            string tableCheckSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='migrations';";
            command.CommandText = tableCheckSql;

            DbDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                return Migration.Empty;
            }

            command = connection.CreateCommand();
            string latestMigration = "SELECT number FROM migrations ORDER BY number DESC LIMIT 1";
            command.CommandText = latestMigration;

            reader = await command.ExecuteReaderAsync();

            int migrationNumber = 0;
            while (reader.Read())
            {
                migrationNumber = reader.GetInt32(0);
            }

            return (Migration)migrationNumber;
        }

        private async Task MigrateSchema(Migration currentMigration, SQLiteConnection connection)
        {
            string migrationText = string.Empty;
            SQLiteCommand command = connection.CreateCommand();

            while (currentMigration < _latestMigration)
            {

                switch (currentMigration)
                {
                    case Migration.Empty:
                        migrationText = @"
    BEGIN TRANSACTION;

    CREATE TABLE migrations (name VARCHAR(50), number INT);

    INSERT INTO migrations (name, number) VALUES ('migration_table', 1);

    COMMIT;";
                        command.CommandText = migrationText;
                        await command.ExecuteNonQueryAsync();

                        currentMigration = Migration.MigrationsTable;
                        break;
                    case Migration.MigrationsTable:
                        migrationText = @"
    BEGIN TRANSACTION;

    CREATE Table sensor_data (date INTEGER, cycleCount INTEGER, accX FLOAT, accY FLOAT, accZ FLOAT, gyroX FLOAT, gyroY FLOAT, gyroZ FLOAT);

    INSERT INTO migrations (name, number) VALUES ('sensor_data_table', 2);

    COMMIT;";
                        command.CommandText = migrationText;
                        await command.ExecuteNonQueryAsync();

                        currentMigration = Migration.SensorData;
                        break;
                    case Migration.SensorData:
                        migrationText = @"
    BEGIN TRANSACTION;

    CREATE Table disc_configuration (deviceId TEXT PRIMARY KEY, date INTEGER, accXOffset FLOAT, accYOffset FLOAT, accZOffset FLOAT, gyroXOffset FLOAT, gyroYOffset FLOAT, gyroZOffset FLOAT);

    INSERT INTO migrations (name, number) VALUES ('disc_configuration_table', 3);

    COMMIT;
";

                        command.CommandText = migrationText;
                        await command.ExecuteNonQueryAsync();

                        currentMigration = Migration.DiscConfiguration;
                        break;
                    default:

                        break;
                }
            }
        }
    }

    public enum Migration
    {
        Empty = 0,
        MigrationsTable = 1,
        SensorData = 2,
        DiscConfiguration = 3
    }
}
