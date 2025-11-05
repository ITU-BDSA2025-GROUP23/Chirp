using System.ComponentModel.DataAnnotations;

namespace Chirp.Infrastructure.DataModel;

public class Author
{
    public required int AuthorId  { get; set; }
    public required string Name { get; set; }
    
    public required string Email { get; set; }
    
    public required ICollection<Cheep>  Cheeps { get; set; }
}
