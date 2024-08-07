# Avalonia with SQLite

## Errors

* `Cannot implicitly convert type 'System.Data.Common.DbDataReader' to 'System.Data.SQLite.SQLiteDataReader'. An explicit conversion exists (are you missing a cast?)`
  * Change your code from `SQLiteDataReader reader = await command.ExecuteReaderAsync();`
  * To this `DbDataReader reader = await command.ExecuteReaderAsync();`

## Resources

* Avalonia SQLite Demo - https://github.com/janbaerts/AvaloniaDBDemo
* Avalonia Fluent Icons - https://avaloniaui.github.io/icons.html
* Sqlite tutorial - https://www.codeguru.com/dotnet/using-sqlite-in-a-c-application/