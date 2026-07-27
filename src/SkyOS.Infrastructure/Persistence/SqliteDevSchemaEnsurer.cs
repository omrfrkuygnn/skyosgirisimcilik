using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SkyOS.Infrastructure.Persistence;

/// <summary>
/// SQLite dev databases cannot run SQL Server-authored EF migrations. This helper patches
/// missing backoffice tables on existing files using SQLite-compatible DDL.
/// </summary>
internal static class SqliteDevSchemaEnsurer
{
    public static async Task EnsureBackofficeTablesAsync(
        SkyOSDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!await TableExistsAsync(context, "AspNetUsers", cancellationToken).ConfigureAwait(false))
        {
            await context.Database.ExecuteSqlRawAsync(IdentityTablesSql, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("SQLite backoffice identity tables ensured.");
        }

        if (!await TableExistsAsync(context, "SiteFeedbacks", cancellationToken).ConfigureAwait(false))
        {
            await context.Database.ExecuteSqlRawAsync(SiteFeedbacksTableSql, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("SQLite SiteFeedbacks table ensured.");
        }

        if (!await TableExistsAsync(context, "AuditLogs", cancellationToken).ConfigureAwait(false))
        {
            await context.Database.ExecuteSqlRawAsync(AuditLogsTableSql, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("SQLite AuditLogs table ensured.");
        }
    }

    private static async Task<bool> TableExistsAsync(
        SkyOSDbContext context,
        string tableName,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (!wasOpen)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private const string IdentityTablesSql = """
        CREATE TABLE IF NOT EXISTS "AspNetRoles" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
            "Name" TEXT NULL,
            "NormalizedName" TEXT NULL,
            "ConcurrencyStamp" TEXT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

        CREATE TABLE IF NOT EXISTS "AspNetUsers" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
            "DisplayName" TEXT NOT NULL,
            "IsActive" INTEGER NOT NULL,
            "UserName" TEXT NULL,
            "NormalizedUserName" TEXT NULL,
            "Email" TEXT NULL,
            "NormalizedEmail" TEXT NULL,
            "EmailConfirmed" INTEGER NOT NULL,
            "PasswordHash" TEXT NULL,
            "SecurityStamp" TEXT NULL,
            "ConcurrencyStamp" TEXT NULL,
            "PhoneNumber" TEXT NULL,
            "PhoneNumberConfirmed" INTEGER NOT NULL,
            "TwoFactorEnabled" INTEGER NOT NULL,
            "LockoutEnd" TEXT NULL,
            "LockoutEnabled" INTEGER NOT NULL,
            "AccessFailedCount" INTEGER NOT NULL
        );
        CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
        CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

        CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
            "RoleId" TEXT NOT NULL,
            "ClaimType" TEXT NULL,
            "ClaimValue" TEXT NULL,
            CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

        CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
            "UserId" TEXT NOT NULL,
            "ClaimType" TEXT NULL,
            "ClaimValue" TEXT NULL,
            CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

        CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
            "LoginProvider" TEXT NOT NULL,
            "ProviderKey" TEXT NOT NULL,
            "ProviderDisplayName" TEXT NULL,
            "UserId" TEXT NOT NULL,
            CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
            CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

        CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
            "UserId" TEXT NOT NULL,
            "RoleId" TEXT NOT NULL,
            CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
            CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

        CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
            "UserId" TEXT NOT NULL,
            "LoginProvider" TEXT NOT NULL,
            "Name" TEXT NOT NULL,
            "Value" TEXT NULL,
            CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
            CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );
        """;

    private const string SiteFeedbacksTableSql = """
        CREATE TABLE IF NOT EXISTS "SiteFeedbacks" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_SiteFeedbacks" PRIMARY KEY AUTOINCREMENT,
            "FullName" TEXT NOT NULL,
            "Email" TEXT NULL,
            "Category" TEXT NOT NULL,
            "Message" TEXT NOT NULL,
            "IsRead" INTEGER NOT NULL,
            "IpAddress" TEXT NULL,
            "Culture" TEXT NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            "UpdatedAtUtc" TEXT NULL
        );
        CREATE INDEX IF NOT EXISTS "IX_SiteFeedbacks_CreatedAtUtc" ON "SiteFeedbacks" ("CreatedAtUtc");
        CREATE INDEX IF NOT EXISTS "IX_SiteFeedbacks_IsRead" ON "SiteFeedbacks" ("IsRead");
        """;

    private const string AuditLogsTableSql = """
        CREATE TABLE IF NOT EXISTS "AuditLogs" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_AuditLogs" PRIMARY KEY AUTOINCREMENT,
            "UserId" TEXT NOT NULL,
            "UserEmail" TEXT NOT NULL,
            "Action" TEXT NOT NULL,
            "EntityType" TEXT NULL,
            "EntityId" TEXT NULL,
            "Details" TEXT NULL,
            "IpAddress" TEXT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            "UpdatedAtUtc" TEXT NULL
        );
        CREATE INDEX IF NOT EXISTS "IX_AuditLogs_CreatedAtUtc" ON "AuditLogs" ("CreatedAtUtc");
        CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
        """;
}
