using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.DataModel;

public class ChatDBContext : IdentityDbContext<IdentityUser>
{
    public ChatDBContext(DbContextOptions<ChatDBContext> options) :
        base(options) { }
    public DbSet<Author>  Authors { get; set; }
    public DbSet<Cheep>  Cheeps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Cheeps)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        
        modelBuilder.Entity<Author>()
            .HasIndex(a => a.AuthorId)
            .IsUnique();
        
        modelBuilder.Entity<Cheep>()
            .HasMany(c => c.Likes)
            .WithMany(a => a.Liked)
            .UsingEntity<Dictionary<string, object>>(
                "CheepLikes",
                    j => j
                        .HasOne<Author>()
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .HasPrincipalKey(a => a.AuthorId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Cheep>()
                        .WithMany()
                        .HasForeignKey("CheepId")
                        .HasPrincipalKey(c => c.CheepId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                        {
                            j.HasKey("AuthorId",  "CheepId");
                            j.ToTable("CheepLikes");
                        }
                        
            );
        
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Followers)
            .WithMany(a => a.Following)
            .UsingEntity<Dictionary<string, object>>(
                "AuthorFollow",
                j => j
                    .HasOne<Author>()
                    .WithMany()
                    .HasForeignKey("FollowerId")
                    .HasPrincipalKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Author>()
                    .WithMany()
                    .HasForeignKey("FolloweeId")
                    .HasPrincipalKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("FollowerId",  "FolloweeId");
                    j.ToTable("AuthorFollow");
                }
                        
            );
        

    }
}