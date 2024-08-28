# Developer Documentation

This file contains all technical documentation for this project.

## Microcontroller

TBD

## Avalonia Client

### Database Conventions

* snake_case for all table names, column names and migration names
### Database Migrations

This project uses SQLite to save data coming from disc. The schema of this database is managed by the [DataSchemaService](./software/client/OpenDataDisc.Services/DataSchemaService.cs).

To create a new migration:

* Create a new entry into the `Migration` enum.
  * Ensure it's value is N + 1, where N is the current highest enum value.
* Add a new case statement in `MigrateSchema` method.
* Write SQL statement that will complete the migration
  * Wrap it in a transaction
  * Ensure you insert a new record into the migration table
    * Provide a unique name in snake case (all_lower_text_and_no_spaces)
* Update the class instance variable `_latestMigration` to match your new migration

Expected test cases. 

> In any database PR please indicate whether you've manually performed these steps.

* No SQLite database
  * Ensure on startup a new database is created with all former migrations and your new one.
* Just Your Migration
  * SQLite database with all former migrations.
  * Ensure on startup your migration is added successfully.
* From Latest Release - Future Test Case
  * Start with database from last official release.
  * Ensure on startup that your migration (and any others added since migration) are completed successfully.
* Schema Update Error Rollback
  * Modify your schema update so it fails.
  * Run and see it error out, ensure that the database schema has not been modified at all.
* Migration Tracking Error Rollback
  * Modify your Migration Tracking sql statement so it fails.
  * Run and see it error out, ensure that the database schema has not been modified at all.


Example for last two test cases:

```sql
BEGIN TRANSACTION;

-- Schema Update
CREATE TABLE migrations (name VARCHAR(50), number INT);

-- Migration Tracking
INSERT INTO migrations (name, number) VALUES ('migration_table', 1);

COMMIT;
```

* This migration has 2 logical parts:
  * Schema Update - schema changes to be made
  * Migration Tracking - adding record into migration table indicating migration was run.

* To test the "Schema Update Error Rollback" test case, change your schema so it fails:
  * `CREATEING TABLE migrations (name VARCHAR(50), number INT);`
  * And ensure rollback is successful
* To test the "Migration Tracking Error Rollback" test case, change the statement so it fails:
  * `INSERTING INTO migrations (name, number) VALUES ('migration_table', 1);`