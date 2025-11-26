using Microsoft.Extensions.Configuration;
using PlanShare.Domain.Enums;

namespace PlanShare.Infrastructure.Extensions;
public static class ConfigurationExtensions
{
    public static string ConnectionString(this IConfiguration configuration)
    {
        var databaseType = configuration.GetDatabaseType();

        if(databaseType == EnumDatabaseType.PostgreSQL)
        {
            return configuration.GetConnectionString("ConnectionPostgreSQL")!;
        }

        if(databaseType == EnumDatabaseType.SQLServer)
        {
            return configuration.GetConnectionString("ConnectionSQLServer")!;
        }

        if(databaseType == EnumDatabaseType.MySQL)
        {
            return configuration.GetConnectionString("ConnectionMySQL")!;
        }

        return configuration.GetConnectionString("Connection")!;
    }

    public static EnumDatabaseType GetDatabaseType(this IConfiguration configuration)
    {
        var databaseType = configuration.GetConnectionString("DatabaseType")!;

        return Enum.Parse<EnumDatabaseType>(databaseType);
    }

    public static bool IsUnitTestEnviroment(this IConfiguration configuration)
    {
        _ = bool.TryParse(configuration.GetSection("InMemoryTests").Value, out bool inMemoryTests);

        return inMemoryTests;
    }
}