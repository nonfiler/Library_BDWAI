using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.Models;

namespace Library.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Library.Models.Book> Book { get; set; } = default!;
        public DbSet<Library.Models.Author> Author { get; set; } = default!;
        
        public DbSet<Library.Models.Comment> Comment { get; set; }
    }
}
