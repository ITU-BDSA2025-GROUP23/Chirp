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
}