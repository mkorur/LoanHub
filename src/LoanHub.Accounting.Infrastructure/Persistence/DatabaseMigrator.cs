using Npgsql;

namespace LoanHub.Accounting.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    public static async Task RunAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Migrations");
        if (!Directory.Exists(dir))
            return;

        var files = Directory.GetFiles(dir, "*.sql").OrderBy(f => f, StringComparer.Ordinal);
        await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);
        foreach (var file in files)
        {
            var sql = await File.ReadAllTextAsync(file, cancellationToken);
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
