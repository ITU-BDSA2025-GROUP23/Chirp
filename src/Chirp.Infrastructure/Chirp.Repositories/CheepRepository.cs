using System.Collections.Generic;
using System.Linq;
using Chirp.Infrastructure.DataModel;
using Chirp.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

    public interface ICheepRepository
    {
        void SaveChanges();
        public IEnumerable<Cheep> GetAllCheeps();

        public IEnumerable<Cheep> GetCheepsByAuthor(string authorName);

        public void AddCheep(string text, Author author);

        public List<Cheep> GetPaginatedCheeps(int currentPage, int pageSize, string? author = null);
        public List<CheepDTO> GetPaginatedCheepsDTO(int currentPage, int pageSize, string? author = null);
        
        int GetCheepCount(string? author = null);
        
        public Author? GetAuthorByName(string userName);
        
        public Author? GetAuthorByEmail(string Email);

        public Author CreateAuthor(string userName, string email);

        public void CreateCheep(string userName, string email, string text);

        public List<Author> GetFollowing(string authorName);
        
        public List<Author> GetFollowers(string authorName);
    }


public class CheepRepository : ICheepRepository
{
    private readonly ChatDBContext _context;

    public CheepRepository(ChatDBContext context)
    {
        _context = context;
    }
    
    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public IEnumerable<Cheep> GetAllCheeps()
    {
        return _context.Cheeps
            .Include(c => c.Author)
            .AsNoTracking()
            .OrderByDescending(c => c.TimeStamp)
            .ThenByDescending(c => c.CheepId)
            .ToList();
    }

    public IEnumerable<Cheep> GetCheepsByAuthor(string authorName)
    {
        return _context.Cheeps
            .Include(c => c.Author)
            .AsNoTracking()
            .Where(c => c.Author != null && c.Author.Name != null && c.Author.Name == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .ThenByDescending(c => c.CheepId)
            .ToList();

    }

    public void AddCheep(string text, Author author)
    {
        if (text.Length > 160 || string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Cheep text exceeds maximum length of 160 characters.");
        }
        var cheep = new Cheep
        {
            Text = text,
            TimeStamp = DateTime.UtcNow,
            Author = author
        };

        _context.Cheeps.Add(cheep);
        _context.SaveChanges();
        
    }

    public int GetCheepCount(string? author = null)
    {
        IQueryable<Cheep> q = _context.Cheeps.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(author))
        {
            var a = author.Trim();
            q = q.Where(c => c.Author != null && c.Author.Name != null && c.Author.Name.Trim().ToLower() == a.ToLower());
        }
        return q.Count();
    }

    public List<Author> GetFollowing(string authorName)
    {
        var author = GetAuthorByName(authorName);
        return author?.Following.ToList() ?? new List<Author>();
    }

    public List<Author> GetFollowers(string authorName)
    {
        var author = GetAuthorByName(authorName);
        return author?.Followers.ToList() ?? new List<Author>();
    }
    
    //remove? - never used
    public List<Cheep> GetPaginatedCheeps(int currentPage, int pageSize, string? author = null)
    {
        if (pageSize < 1) pageSize = 32;
        if (currentPage < 1) currentPage = 1;

        IQueryable<Cheep> q = _context.Cheeps
            .Include(c => c.Author)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(author))
        {
            q = q.Where(c => c.Author != null && c.Author.Name != null && c.Author.Name == author);        
        }
        return q
            .OrderByDescending(c => c.TimeStamp)
            .ThenByDescending(c => c.CheepId)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        }

    public List<CheepDTO> GetPaginatedCheepsDTO(int currentPage, int pageSize, string? author = null)
    {
        if (pageSize < 1) pageSize = 32;
        if (currentPage < 1) currentPage = 1;

        var q = _context.Cheeps
            .Include(c => c.Author)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
        {
            var a = author.Trim();
            q = q.Where(c => c.Author != null && c.Author.Name != null && c.Author.Name.Trim().ToLower() == author.ToLower());
        }
        var rows = q.OrderByDescending(c => c.TimeStamp)
                    .ThenByDescending(c => c.CheepId)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.CheepId,
                        c.Text,
                        c.TimeStamp,
                        AuthorName = c.Author!.Name
                    })
                    .ToList();

        return rows.Select(r => new CheepDTO
            {
                Id = r.CheepId,
                Text = r.Text,
                AuthorName = r.AuthorName,
                TimeIso = r.TimeStamp.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();
    }
    
    public Author? GetAuthorByName(string userName)
    {
        return _context.Authors
            .Include(a => a.Followers)
            .Include(a => a.Following)
            .FirstOrDefault(a => a.Name == userName);
    }
    
    public Author? GetAuthorByEmail(string email)
    {
        return _context.Authors
            .FirstOrDefault(a => a.Email == email);
    }
    
    public Author CreateAuthor(string userName, string email)
    {
        var author = new Author
        {
            Name = userName,
            Email = email,
            Cheeps = new List<Cheep>()
        };

        _context.Authors.Add(author);
        _context.SaveChanges();
        
        return author;
    }

    public void CreateCheep(string userName, string email, string text)
    {
        text = text.Trim();
        if (text.Length > 160 || string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Cheep text exceeds maximum length of 160 characters.");
        }
        
        var author = GetAuthorByName(userName)
            ?? GetAuthorByEmail(email);

        if (author == null)
        {
            author = CreateAuthor(userName, email);
        }

        var cheep = new Cheep
        {
            Text = text,
            TimeStamp = DateTime.UtcNow,
            Author = author

        };

        _context.Cheeps.Add(cheep);
        _context.SaveChanges();
    } 
}



