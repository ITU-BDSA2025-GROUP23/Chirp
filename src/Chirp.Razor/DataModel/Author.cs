using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.DataModel;

public class Author
{
    public required int id  { get; set; }
    public required string Username { get; set; }
    
    public required string Email { get; set; }
    
    public required ICollection<Cheep>  Cheeps { get; set; }
}
