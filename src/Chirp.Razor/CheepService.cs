/*
using Chirp.Razor.DataModel;

namespace Chirp.Razor;
using Microsoft.EntityFrameworkCore;

public interface ICheepService
{
    public List<Cheep> GetCheeps(string? author = null);
    public List<Cheep> GetPaginatedCheeps(int currentPage, int pageSize, string? author = null);
}

public class CheepService : ICheepService
{
    readonly  ChatDBContext _context;
    readonly ILogger _logger;
    
    public CheepService(ChatDBContext context, ILoggerFactory factory)
    {
        _context = context;
        _logger = factory.CreateLogger<CheepService>();
    }
	public List<Cheep> GetCheeps(string? author = null)
    {
        throw new NotImplementedException();
    }

    public List<Cheep> GetPaginatedCheeps(int currentPage, int pageSize, string? author = null)
    {
        throw new NotImplementedException();
    }
}
*/