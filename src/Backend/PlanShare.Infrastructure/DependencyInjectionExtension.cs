using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlanShare.Domain.Enums;
using PlanShare.Domain.Repositories;
using PlanShare.Domain.Repositories.Association;
using PlanShare.Domain.Repositories.User;
using PlanShare.Domain.Repositories.WorkItem;
using PlanShare.Domain.Security.Cryptography;
using PlanShare.Domain.Security.Tokens;
using PlanShare.Domain.Services.LoggedUser;
using PlanShare.Infrastructure.DataAccess;
using PlanShare.Infrastructure.DataAccess.Repositories;
using PlanShare.Infrastructure.Extensions;
using PlanShare.Infrastructure.Security.Cryptography;
using PlanShare.Infrastructure.Security.Tokens.Access.Generator;
using PlanShare.Infrastructure.Security.Tokens.Access.Validator;
using PlanShare.Infrastructure.Services.LoggedUser;
using System.Reflection;

namespace PlanShare.Infrastructure;
public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddRepositories(services);
        AddLoggedUser(services);
        AddTokenHandlers(services, configuration);
        AddPasswordEncripter(services);
        AddDbContext(services, configuration);
        AddFluentMigrator(services, configuration);
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.ConnectionString();

        services.AddDbContext<PlanShareDbContext>(dbContextOptions =>
        {
            var dataBaseType = configuration.GetDatabaseType();

            dbContextOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            if (dataBaseType == EnumDatabaseType.SQLServer)
            {
                dbContextOptions.UseSqlServer(connectionString);
            }
            else if (dataBaseType == EnumDatabaseType.MySQL)
            {
                dbContextOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
            else if (dataBaseType == EnumDatabaseType.PostgreSQL)
            {
                dbContextOptions.UseNpgsql(connectionString);
            }
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserWriteOnlyRepository, UserRepository>();
        services.AddScoped<IUserReadOnlyRepository, UserRepository>();
        services.AddScoped<IUserUpdateOnlyRepository, UserRepository>();

        services.AddScoped<IWorkItemWriteOnlyRepository, WorkItemRepository>();
        services.AddScoped<IWorkItemReadOnlyRepository, WorkItemRepository>();
        services.AddScoped<IWorkItemUpdateOnlyRepository, WorkItemRepository>();

        services.AddScoped<IPersonAssociationReadOnlyRepository, PersonAssociationRepository>();
    }

    private static void AddLoggedUser(IServiceCollection services) => services.AddScoped<ILoggedUser, LoggedUser>();

    private static void AddPasswordEncripter(IServiceCollection services)
    {
        services.AddScoped<IPasswordEncripter, BCryptNet>();
    }

    private static void AddTokenHandlers(IServiceCollection services, IConfiguration configuration)
    {
        var expirationTimeMinutes = configuration.GetValue<uint>("Settings:Jwt:ExpiresMinutes");
        var signingKey = configuration.GetValue<string>("Settings:Jwt:SigningKey")!;

        services.AddScoped<IAccessTokenValidator>(option => new JwtTokenValidator(signingKey));
        services.AddScoped<IAccessTokenGenerator>(option => new JwtTokenGenerator(expirationTimeMinutes, signingKey));
    }

    private static void AddFluentMigrator(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.ConnectionString();
        var dataBaseType = configuration.GetDatabaseType();

        services.AddFluentMigratorCore()
            .ConfigureRunner(config =>
            {
                IMigrationRunnerBuilder migrationRunnerBuilder = null!;
                var infrastructureAssembly = Assembly.Load("PlanShare.Infrastructure");

                if (dataBaseType == EnumDatabaseType.MySQL)
                {
                    migrationRunnerBuilder = config
                        .AddMySql5();
                }
                else if(dataBaseType == EnumDatabaseType.SQLServer)
                {
                    migrationRunnerBuilder = config
                        .AddSqlServer();
                }
                else if(dataBaseType == EnumDatabaseType.PostgreSQL)
                {
                    migrationRunnerBuilder = config
                        .AddPostgres();
                }

                migrationRunnerBuilder
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(infrastructureAssembly)
                    .For.All();
            });
    }
}
