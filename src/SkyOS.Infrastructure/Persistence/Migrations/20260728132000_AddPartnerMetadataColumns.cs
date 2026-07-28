using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyOS.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[DbContext(typeof(SkyOSDbContext))]
[Migration("20260728132000_AddPartnerMetadataColumns")]
public partial class AddPartnerMetadataColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Address",
            table: "Partners",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Partners",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SupportLetterUrl",
            table: "Partners",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Address",
            table: "Partners");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Partners");

        migrationBuilder.DropColumn(
            name: "SupportLetterUrl",
            table: "Partners");
    }
}
