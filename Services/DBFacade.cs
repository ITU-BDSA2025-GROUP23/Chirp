using System.Data;
using Microsoft.Data.Sqlite;

namespace Chirp.Razor.Services;

public sealed class DBFacade
{
    private readonly string _connectionString;

    // Accept the path via DI; fall back to temp dir if env var missing.
    public DBFacade(string? dbPath)
    {
        var path = string.IsNullOrWhiteSpace(dbPath)
            ? Path.Combine(Path.GetTempPath(), "chirp.db")
            : dbPath!;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public IDbConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();
        return conn;
    }
}