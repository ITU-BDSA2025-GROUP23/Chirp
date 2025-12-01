using System.ComponentModel.DataAnnotations;

namespace Chirp.Infrastructure.DataModel;

public class Author
{ 
    public int AuthorId  { get; set; }
    public required string Name { get; set; }
    
    public required string Email { get; set; }
    
    public required ICollection<Cheep>  Cheeps { get; set; }
    
    public ICollection<Author> Followers { get; set; } = new List<Author>();
    
    public ICollection<Author> Following { get; set; } = new List<Author>();

    public void Follow(Author author)
    {
        if (!Following.Contains(author))
            Following.Add(author);

        if (!author.Followers.Contains(this))
            author.Followers.Add(this);
    }
}
