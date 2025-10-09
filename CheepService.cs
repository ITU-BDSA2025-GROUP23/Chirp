using Chirp.Razor.Services;
using Microsoft.Data.Sqlite;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps();
    List<CheepViewModel> GetCheepsFromAuthor(string author);

    // NEW: paged overloads
    List<CheepViewModel> GetCheeps(int page, int pageSize);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page, int pageSize);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _db;
    public CheepService(DBFacade db) => _db = db;

    public List<CheepViewModel> GetCheeps() => GetCheeps(1, 32);
    public List<CheepViewModel> GetCheepsFromAuthor(string author) => GetCheepsFromAuthor(author, 1, 32);

    public List<CheepViewModel> GetCheeps(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;
        var offset = (page - 1) * pageSize;

        var list = new List<CheepViewModel>();
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC, m.message_id DESC
            LIMIT $limit OFFSET $offset;";
        var pLimit = cmd.CreateParameter(); pLimit.ParameterName = "$limit"; pLimit.Value = pageSize; cmd.Parameters.Add(pLimit);
        var pOffset = cmd.CreateParameter(); pOffset.ParameterName = "$offset"; pOffset.Value = offset; cmd.Parameters.Add(pOffset);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CheepViewModel(
                reader.GetString(0),
                reader.GetString(1),
                Unix(reader.GetInt64(2))
            ));
        }
        return list;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;
        var offset = (page - 1) * pageSize;

        var list = new List<CheepViewModel>();
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = $author
            ORDER BY m.pub_date DESC, m.message_id DESC
            LIMIT $limit OFFSET $offset;";
        var pAuthor = cmd.CreateParameter(); pAuthor.ParameterName = "$author"; pAuthor.Value = author; cmd.Parameters.Add(pAuthor);
        var pLimit = cmd.CreateParameter(); pLimit.ParameterName = "$limit"; pLimit.Value = pageSize; cmd.Parameters.Add(pLimit);
        var pOffset = cmd.CreateParameter(); pOffset.ParameterName = "$offset"; pOffset.Value = offset; cmd.Parameters.Add(pOffset);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CheepViewModel(
                reader.GetString(0),
                reader.GetString(1),
                Unix(reader.GetInt64(2))
            ));
        }
        return list;
    }

    private static string Unix(long s)
        => DateTimeOffset.FromUnixTimeSeconds(s).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
}