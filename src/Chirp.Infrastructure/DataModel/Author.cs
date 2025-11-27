using System.ComponentModel.DataAnnotations;

namespace Chirp.Infrastructure.DataModel;

public class Author
{ 
    public int AuthorId  { get; set; }
    public required string Name { get; set; }
    
    public required string Email { get; set; }
    
    [Unique]
    public required ICollection<Cheep>  Cheeps { get; set; }
    
    public ICollection<Author> Followers { get; set; }
    
    public ICollection<Author> Following { get; set; }

    
    public void Follow(Author author)
    {
        if (!Following.Contains(author))
        {
            Following.Add(author);
        }
    }

    
    public void Follows(Auhtor author)
    {
        if (!Followers.Contains(author))
        {
            Followers.Add(author);
        }
    }
}
