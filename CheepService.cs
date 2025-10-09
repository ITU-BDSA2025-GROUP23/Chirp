using System.Data;
using Chirp.Razor.Services;
using Microsoft.Data.Sqlite;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps();
    List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _db;

    public CheepService(DBFacade db)
    {
        _db = db;
    }

    public List<CheepViewModel> GetCheeps()
    {
        var cheeps = new List<CheepViewModel>();

        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC
            LIMIT 32;
        ";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var author = reader.GetString(0);
            var message = reader.GetString(1);
            var timestamp = UnixTimeStampToDateTimeString(reader.GetInt64(2));
            cheeps.Add(new CheepViewModel(author, message, timestamp));
        }

        return cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        var cheeps = new List<CheepViewModel>();

        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = $author
            ORDER BY m.pub_date DESC
            LIMIT 32;
        ";
        var param = cmd.CreateParameter();
        param.ParameterName = "$author";
        param.Value = author;
        cmd.Parameters.Add(param);        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            var username = reader.GetString(0);
            var message = reader.GetString(1);
            var timestamp = UnixTimeStampToDateTimeString(reader.GetInt64(2));
            cheeps.Add(new CheepViewModel(username, message, timestamp));
        }

        return cheeps;
    }

    private static string UnixTimeStampToDateTimeString(long unixTimeStamp)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).ToLocalTime().DateTime;
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}