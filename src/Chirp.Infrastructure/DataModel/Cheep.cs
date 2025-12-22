using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Infrastructure.DataModel;

public class Cheep
{
    public int CheepId  { get; set; }
	public int AuthorId { get; set; }
	public Author? Author { get; set; } = null!;
	
	[Required]
	[StringLength(160)] 
	public string Text { get; set; } = default!;
	
    public required DateTime TimeStamp  { get; set; }
    
    public ICollection<Author> Likes  { get; set; } = new List<Author>();
  
}