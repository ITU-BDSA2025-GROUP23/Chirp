using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;

public interface IDatabaseRepository<T>
{
    IEnumerable<T> Read(int? limit = null);
    void Store(T record);
}

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _filePath;
    private readonly CsvConfiguration _config;

    public CSVDatabase(string filepath)
    {
        _filePath = filepath;
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        if (!File.Exists(_filePath))
        {
            return Enumerable.Empty<T>();
        }

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, _config);

        var records = csv.GetRecords<T>();
        if (limit.HasValue)
        {
            return records.Take(limit.Value).ToList();
        }
        else
        {
            return records.ToList();
        }
    }

    public void Store(T record)
    {
        bool fileExists = File.Exists(_filePath);

        using var stream = new StreamWriter(_filePath, append: true);
        using var csv = new CsvWriter(stream, _config);

        if (!fileExists)
        {
            csv.WriteHeader<T>();
            csv.NextRecord();
        }

        csv.WriteRecord(record);
        csv.NextRecord();
    }

}
