# Avalonia with SQLite

I am probably going to use SQLite to store throw data. I need to know how to integrate that into avalonia.

I copied from example 17, imported the sqlite nuget package. Then integrated it into the `MainWindowViewModel.cs`.

## To run locally

* Open in Visual Studio
* Run applicatin
* Click top right "notebook" button
* That button creates the database, creates a table, inserts records, then selects any records from there.
  * If running multiple times, ensure you comment out the table creation
## Errors

* `Cannot implicitly convert type 'System.Data.Common.DbDataReader' to 'System.Data.SQLite.SQLiteDataReader'. An explicit conversion exists (are you missing a cast?)`
  * Change your code from `SQLiteDataReader reader = await command.ExecuteReaderAsync();`
  * To this `DbDataReader reader = await command.ExecuteReaderAsync();`

## Resources

* Avalonia SQLite Demo - https://github.com/janbaerts/AvaloniaDBDemo
* Avalonia Fluent Icons - https://avaloniaui.github.io/icons.html
* Sqlite tutorial - https://www.codeguru.com/dotnet/using-sqlite-in-a-c-application/