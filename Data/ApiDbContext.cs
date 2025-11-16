using Microsoft.EntityFrameworkCore;
using project_pastebin.Models; // Supaya dia kenal sama 'PasteContent'

namespace project_pastebin.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        // Ini jembatan ke tabel PasteContents di database
        public DbSet<PasteContent> PasteContents { get; set; }
        public DbSet<User> Users { get; set; }
    }
}