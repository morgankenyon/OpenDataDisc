using OpenDataDisc.Services.Interfaces;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OpenDataDisc.Services
{
    internal class DataSchemaService : IDataSchemaService
    {
        private readonly Migration _newMigration = Migration.SensorData;
        public async Task MigrateSchemaToLatest()
        {
            try
            {

                SQLiteConnection sqlConn = new SQLiteConnection("Data Source=opendatadisc.db;Version=3;New=True;Compress=True");

                sqlConn.Open();

                var currentMigration = await CalculateLatestMigration(sqlConn);

                if (currentMigration < _newMigration)
                {
                    await MigrateSchema(currentMigration, sqlConn);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                string nextMsg = $"{msg}";
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

        private async Task MigrateSchema(Migration latestMigration, SQLiteConnection connection)
        {
            string migrationText = string.Empty;
            SQLiteCommand command = connection.CreateCommand();

            while (latestMigration < _newMigration)
            {

                switch (latestMigration)
                {
                    case Migration.Empty:
                        migrationText = @"
    BEGIN TRANSACTION;

    CREATE TABLE migrations (name VARCHAR(50), number INT);

    INSERT INTO migrations (name, number) VALUES ('migration_table', 1);

    COMMIT;";
                        command.CommandText = migrationText;
                        await command.ExecuteNonQueryAsync();

                        latestMigration = Migration.MigrationsTable;
                        break;
                    case Migration.MigrationsTable:
                        migrationText = @"
    BEGIN TRANSACTION;

    CREATE Table sensor_data (type VARCHAR(20), date INTEGER, val1 FLOAT, val2 FLOAT, val3 FLOAT);

    INSERT INTO migrations (name, number) VALUES ('sensor_data table', 2);

    COMMIT;";
                        command.CommandText = migrationText;
                        await command.ExecuteNonQueryAsync();

                        latestMigration = Migration.SensorData;
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
    }
}
