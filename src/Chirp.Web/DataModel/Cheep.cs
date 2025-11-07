using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Web.DataModel;

public class Cheep
{
    public int CheepId  { get; set; }
	public required int AuthorId { get; set; }
	public Author? Author { get; set; } = null!;
    public required string Text { get; set; } = default!;
    public required DateTime TimeStamp  { get; set; }
  
}