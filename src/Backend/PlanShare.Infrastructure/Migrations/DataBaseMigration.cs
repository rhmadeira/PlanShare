using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using PlanShare.Domain.Enums;

namespace PlanShare.Infrastructure.Migrations;
public static class DataBaseMigration
{

    public static void Migrate(EnumDatabaseType dataBaseType, string connectionString, IServiceProvider serviceProvider)
    {
        if (dataBaseType is EnumDatabaseType.PostgreSQL)
            EnsureDatabaseCreatedForPostgreSQL(connectionString);
        if (dataBaseType is EnumDatabaseType.SQLServer)
            EnsureDatabaseCreatedForSQLServer(connectionString);
        if (dataBaseType is EnumDatabaseType.MySQL)
            EnsureDatabaseCreatedForMySQL(connectionString);

        MigrateDatabase(serviceProvider);
    }
    private static void EnsureDatabaseCreatedForSQLServer(string connectionString)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        var databaseName = connectionStringBuilder.InitialCatalog;

        connectionStringBuilder.Remove("Initial Catalog");

        using var dbConnection = new SqlConnection(connectionStringBuilder.ConnectionString);

        var parameters = new DynamicParameters();

        parameters.Add("databaseName", databaseName);

        var records = dbConnection.Query("SELECT * FROM sys.databases WHERE name = @databaseName",
             parameters);

        if (records.Any() == false)
            dbConnection.Execute($"CREATE DATABASE {databaseName}");

    }
    private static void EnsureDatabaseCreatedForPostgreSQL(string connectionString)
    {
        // Implementation for PostgreSQL database creation if needed
    }
    private static void EnsureDatabaseCreatedForMySQL(string connectionString)
    {
        var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;

        connectionStringBuilder.Remove("Database");

        using var dbConnection = new MySqlConnection(connectionStringBuilder.ConnectionString);

        dbConnection.Execute($"CREATE DATABASE IF NOT EXISTS {databaseName}");
    }
    private static void MigrateDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        runner.ListMigrations();

        runner.MigrateUp();
    }
}
