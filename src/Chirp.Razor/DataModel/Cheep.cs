using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Razor.DataModel;

public class Cheep
{
    public int id  { get; set; }
    public required string Text { get; set; } = default!;
    public required DateTime Timestamp  { get; set; }
    
    public int AuthorId { get; set; }
    public Author? Author { get; set; } = null!;
}