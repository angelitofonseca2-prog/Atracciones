using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atracciones.BuildingBlocks.Database;

/// <summary>
/// Evita 42P07 ("relation already exists") cuando las tablas existen pero el historial EF
/// está en otro esquema (p. ej. public) o vacío tras cambiar MigrationsHistoryTable.
/// </summary>
public static class EfMigrationHistoryBaseline
{
    public const string DefaultProductVersion = "10.0.5";

    public static async Task AlignHistoryIfTablesExistAsync(
        DbContext db,
        string historySchema,
        string markerSchema,
        string markerTable,
        IReadOnlyList<string> migrationIds,
        ILogger? logger = null,
        string productVersion = DefaultProductVersion,
        CancellationToken cancellationToken = default)
    {
        if (migrationIds.Count == 0)
            return;

        ValidateIdentifier(historySchema);
        ValidateIdentifier(markerSchema);
        ValidateIdentifier(markerTable);

#pragma warning disable EF1002 // historySchema validado con ValidateIdentifier
        await db.Database.ExecuteSqlRawAsync(
            $"""
            CREATE SCHEMA IF NOT EXISTS "{historySchema}";

            CREATE TABLE IF NOT EXISTS "{historySchema}"."__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );

            INSERT INTO "{historySchema}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT p."MigrationId", p."ProductVersion"
            FROM public."__EFMigrationsHistory" p
            WHERE EXISTS (
                SELECT 1 FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
            )
            ON CONFLICT ("MigrationId") DO NOTHING;
            """,
            cancellationToken);
#pragma warning restore EF1002

        foreach (var migrationId in migrationIds)
        {
            var insertSql =
                "INSERT INTO \"" + historySchema + "\".\"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                "SELECT {0}, {1} " +
                "WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = {2} AND table_name = {3}) " +
                "AND NOT EXISTS (SELECT 1 FROM \"" + historySchema + "\".\"__EFMigrationsHistory\" h WHERE h.\"MigrationId\" = {0});";
            await db.Database.ExecuteSqlRawAsync(
                insertSql,
                migrationId,
                productVersion,
                markerSchema,
                markerTable,
                cancellationToken);
        }

        logger?.LogInformation(
            "Historial EF revisado en esquema {Schema} (marcador {MarkerSchema}.{MarkerTable}).",
            historySchema,
            markerSchema,
            markerTable);
    }

    public static async Task MigrateWithBaselineAsync(
        DbContext db,
        string historySchema,
        string markerSchema,
        string markerTable,
        IReadOnlyList<string> migrationIds,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        await AlignHistoryIfTablesExistAsync(
            db, historySchema, markerSchema, markerTable, migrationIds, logger, cancellationToken: cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }

    private static void ValidateIdentifier(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !id.All(static c => char.IsLetterOrDigit(c) || c == '_'))
            throw new ArgumentException($"Identificador SQL inválido: {id}", nameof(id));
    }
}
