namespace Chirp.Razor.DTOs;

public class CheepDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public string TimeIso { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
}