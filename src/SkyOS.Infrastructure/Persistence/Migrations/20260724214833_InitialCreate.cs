using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SkyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    InterestType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DateAchieved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVerifiedRelationship = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsLeader = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Milestones",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "DateAchieved", "Description", "DisplayOrder", "Title", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "Teknofest", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sürü İHA kategorisinde gerçek uçuş testleri gerçekleştirildi.", 1, "TEKNOFEST — Sürü İHA", null },
                    { 2, "Teknofest", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Savaşan İHA kategorisinde otonom hedef kilitleme algoritmaları sahada test edildi.", 2, "TEKNOFEST — Savaşan İHA", null },
                    { 3, "Teknik", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hava, kara (simülasyon) ve deniz (simülasyon) olmak üzere üç farklı araç tipiyle entegrasyon doğrulandı.", 3, "Çoklu Platform Entegrasyonu", null },
                    { 4, "Teknik", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ArduPilot ve DJI ile karşılaştırmalı performans ölçümleri yapıldı.", 4, "Karşılaştırmalı Performans Ölçümü", null },
                    { 5, "Kurumsal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "HAVELSAN ile teknik değerlendirme görüşmesi yapıldı ve Jet Cube hızlandırıcı programına kabul edildi.", 5, "HAVELSAN Jet Cube Programı", null }
                });

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "DisplayOrder", "IsVerifiedRelationship", "LogoUrl", "Name", "UpdatedAtUtc", "WebsiteUrl" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HAVELSAN ile teknik değerlendirme görüşmesi gerçekleştirildi ve Jet Cube hızlandırıcı programına kabul edildi.", 1, true, null, "HAVELSAN", null, null });

            migrationBuilder.InsertData(
                table: "TeamMembers",
                columns: new[] { "Id", "Bio", "CreatedAtUtc", "DisplayOrder", "FullName", "IsLeader", "PhotoUrl", "Role", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Yunus Emre Gözalıcı", true, null, "Ekip Lideri", null },
                    { 2, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Enver Sabri Özkartal", true, null, "Ekip Lideri", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_IpAddress_CreatedAtUtc",
                table: "ContactMessages",
                columns: new[] { "IpAddress", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_IsRead",
                table: "ContactMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_Category",
                table: "Milestones",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_DisplayOrder",
                table: "Milestones",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_IsPublished",
                table: "NewsItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_Slug",
                table: "NewsItems",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_DisplayOrder",
                table: "Partners",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_DisplayOrder",
                table: "TeamMembers",
                column: "DisplayOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "NewsItems");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "TeamMembers");
        }
    }
}
