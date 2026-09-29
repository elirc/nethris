using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nethris.Api.Domain;

namespace Nethris.Api.Data;

/// <summary>Owns schema identity and creates independent EF units of work.</summary>
public sealed class HrisDatabase
{
    private const int ApplicationId = 1313362505;
    public string Path { get; }
    public string ConnectionString { get; }
    public string SigningKey { get; }

    public HrisDatabase(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        var exists = File.Exists(Path);
        ConnectionString = new SqliteConnectionStringBuilder
        { DataSource = Path, ForeignKeys = true, Pooling = false, DefaultTimeout = 5 }.ToString();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(deferred: false);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA application_id";
        var marker = Convert.ToInt64(command.ExecuteScalar());
        command.CommandText = "PRAGMA user_version";
        var version = Convert.ToInt64(command.ExecuteScalar());
        if (exists && (marker != ApplicationId || version != 1))
            throw new InvalidDataException("Unknown HRIS database. Choose a new Hris__Database path or perform an explicitly reviewed migration; existing data was not reset.");
        using var db = new HrisDbContext(new DbContextOptionsBuilder<HrisDbContext>().UseSqlite(connection).Options);
        db.Database.UseTransaction(transaction);
        if (!exists)
        {
            db.Database.ExecuteSqlRaw(db.Database.GenerateCreateScript());
            db.WorkshopSettings.Add(new WorkshopSettings
            { Name = "Uninitialized HRIS workshop", SigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)) });
            db.SaveChanges();
            command.CommandText = $"PRAGMA application_id={ApplicationId}; PRAGMA user_version=1;";
            command.ExecuteNonQuery();
        }
        SigningKey = db.WorkshopSettings.AsNoTracking().Single(s => s.Id == 1).SigningKey;
        if (SigningKey.Length != 64 || !SigningKey.All(Uri.IsHexDigit)) throw new InvalidDataException("Invalid private signing material.");
        transaction.Commit();
    }

    public HrisDbContext Create() => new(new DbContextOptionsBuilder<HrisDbContext>().UseSqlite(ConnectionString).Options);
}
