using FluentMigrator;

namespace PlanShare.Infrastructure.Migrations.Versions;

[Migration(DataBaseVersions.TABLE_TARGET_VERSION, "Create table to save the user's information" )]
public class Version00000001 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable().Unique()
            .WithColumn("Password").AsString(2000).NotNullable()
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedOn").AsDateTime().NotNullable();
    }
}
