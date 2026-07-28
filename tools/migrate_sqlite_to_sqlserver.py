import json
import sqlite3
import subprocess
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SQLITE_DB = ROOT / "src" / "SkyOS.Web" / "skyos.dev.db"
SQL_SERVER = r"(localdb)\MSSQLLocalDB"
SQL_DATABASE = "SkyOSLocalDev"

TABLE_ORDER = [
    "AspNetRoles",
    "AspNetUsers",
    "AspNetRoleClaims",
    "AspNetUserClaims",
    "AspNetUserLogins",
    "AspNetUserRoles",
    "AspNetUserTokens",
    "TeamMembers",
    "Partners",
    "Milestones",
    "NewsItems",
    "ContactMessages",
    "SiteFeedbacks",
    "AuditLogs",
]

DELETE_ORDER = [
    "AuditLogs",
    "SiteFeedbacks",
    "ContactMessages",
    "NewsItems",
    "Milestones",
    "Partners",
    "TeamMembers",
    "AspNetUserTokens",
    "AspNetUserRoles",
    "AspNetUserLogins",
    "AspNetUserClaims",
    "AspNetRoleClaims",
    "AspNetUsers",
    "AspNetRoles",
]

IDENTITY_TABLES = {
    "AspNetRoleClaims",
    "AspNetUserClaims",
    "TeamMembers",
    "Partners",
    "Milestones",
    "NewsItems",
    "ContactMessages",
    "SiteFeedbacks",
    "AuditLogs",
}


def sql_literal(value):
    if value is None:
        return "NULL"
    if isinstance(value, bool):
        return "1" if value else "0"
    if isinstance(value, int):
        return str(value)
    if isinstance(value, float):
        return repr(value)

    text = str(value).replace("'", "''")
    return f"N'{text}'"


def fetch_sqlite_counts(connection):
    cursor = connection.cursor()
    tables = [
        row[0]
        for row in cursor.execute(
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name
            """
        ).fetchall()
    ]

    return {
        table: cursor.execute(f"SELECT COUNT(*) FROM [{table}]").fetchone()[0]
        for table in tables
    }


def fetch_rows(connection, table_name):
    cursor = connection.cursor()
    rows = cursor.execute(f"SELECT * FROM [{table_name}]").fetchall()
    columns = [column[0] for column in cursor.description]
    return columns, rows


def generate_migration_sql(connection):
    existing_tables = set(fetch_sqlite_counts(connection).keys())
    statements = [
        "SET NOCOUNT ON;",
        "SET ANSI_NULLS ON;",
        "SET QUOTED_IDENTIFIER ON;",
        "BEGIN TRANSACTION;",
    ]

    for table_name in DELETE_ORDER:
        statements.append(f"DELETE FROM [{table_name}];")

    for table_name in TABLE_ORDER:
        if table_name not in existing_tables:
            continue

        columns, rows = fetch_rows(connection, table_name)
        if not rows:
            continue

        column_list = ", ".join(f"[{column}]" for column in columns)
        if table_name in IDENTITY_TABLES:
            statements.append(f"SET IDENTITY_INSERT [{table_name}] ON;")

        for row in rows:
            values = ", ".join(sql_literal(value) for value in row)
            statements.append(
                f"INSERT INTO [{table_name}] ({column_list}) VALUES ({values});"
            )

        if table_name in IDENTITY_TABLES:
            variable_name = f"@maxId_{table_name}"
            statements.append(f"SET IDENTITY_INSERT [{table_name}] OFF;")
            statements.append(
                f"DECLARE {variable_name} INT; "
                f"SELECT {variable_name} = ISNULL(MAX([Id]), 0) FROM [{table_name}]; "
                f"DBCC CHECKIDENT ('[{table_name}]', RESEED, {variable_name}) WITH NO_INFOMSGS;"
            )

    statements.append("COMMIT TRANSACTION;")
    return "\n".join(statements)


def run_sqlcmd(sql_file):
    command = [
        "sqlcmd",
        "-S",
        SQL_SERVER,
        "-E",
        "-C",
        "-d",
        SQL_DATABASE,
        "-b",
        "-f",
        "65001",
        "-i",
        str(sql_file),
    ]
    result = subprocess.run(command, capture_output=True, text=True, encoding="utf-8")
    return result.returncode, result.stdout, result.stderr


def main():
    if not SQLITE_DB.exists():
        print(f"SQLite database not found: {SQLITE_DB}", file=sys.stderr)
        sys.exit(1)

    with sqlite3.connect(SQLITE_DB) as sqlite_connection:
        counts = fetch_sqlite_counts(sqlite_connection)
        print(json.dumps(counts, ensure_ascii=False, indent=2))

        migration_sql = generate_migration_sql(sqlite_connection)

    # UTF-8 BOM helps sqlcmd detect Unicode input reliably on Windows.
    with tempfile.NamedTemporaryFile(
        mode="w", encoding="utf-8-sig", suffix=".sql", delete=False, newline="\n"
    ) as temp_sql:
        temp_sql.write(migration_sql)
        temp_path = Path(temp_sql.name)

    exit_code, stdout, stderr = run_sqlcmd(temp_path)
    temp_path.unlink(missing_ok=True)

    if stdout.strip():
        print(stdout.strip())
    if stderr.strip():
        print(stderr.strip(), file=sys.stderr)

    if exit_code != 0:
        sys.exit(exit_code)


if __name__ == "__main__":
    main()
