using System.Collections.Generic;
using System.Linq;
using Chirp.Infrastructure.DataModel;
using Chirp.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;


public interface IAuthorRepository
{
    void SaveChanges();
    public void Follow(Author author,Author wantFollow);
    
    public void UnFollow(Author author,Author wantunFollow);
    
    public void DeleteLikeFromAuthor(Author author, Cheep cheep);

    public void DeleteAuthor(Author author);

}


public class AuthorRepository : IAuthorRepository
{
    private readonly ChatDBContext _context;
    
    public AuthorRepository(ChatDBContext context)
    {
        _context = context;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void Follow(Author author,Author wantFollow)
    {
        if (author != null && wantFollow != null)
        {
            author.Following.Add(wantFollow);
            wantFollow.Followers.Add(author);    
        }
    }

    public void UnFollow(Author author, Author wantunFollow)
    {
        if (author != null && wantunFollow != null)
        {
            author.Following.Remove(wantunFollow);
            wantunFollow.Followers.Remove(author);    
        }
    }
    
    public void DeleteLikeFromAuthor(Author author, Cheep cheep)
    {
        _context.Entry(author).Collection(a => a.Liked).Load();
        _context.Entry(cheep).Collection(c => c.Likes).Load();

        if (author.Liked == null || cheep.Likes == null) 
            return;
        
        if (author.Liked.Contains(cheep) && cheep.Likes.Contains(author))
        {
            author.Liked.Remove(cheep);
            cheep.Likes.Remove(author);
            SaveChanges();
        }
    }


    public void DeleteAuthor(Author author)
    {
        author.Name = "Deleted user"; 
        author.Email = $"{Guid.NewGuid()}@deleted.local"; 
        _context.Authors.Update(author);
        
        _context.Entry(author).Collection(a => a.Cheeps).Load();
        _context.Entry(author).Collection(a => a.Following).Load();
        _context.Entry(author).Collection(a => a.Liked).Load();
        
        foreach (var cheep in author.Cheeps)
        {
            cheep.Text = "*This take has been deleted*";
            
            _context.Entry(cheep).Collection(c => c.Likes).Load();
            cheep.Likes.Clear();
        }

        foreach (var followed in author.Following.ToList())
        {
            UnFollow(author, followed);
        }
        
        foreach (var follower in author.Followers.ToList())
        {
            UnFollow(follower, author);
        }
        
        foreach (var cheepDislike in author.Liked.ToList())
        {
            DeleteLikeFromAuthor(author, cheepDislike);
        }
        SaveChanges();
    }
}