using Microsoft.EntityFrameworkCore;
using UserManagementApp.Models;

namespace UserManagementApp.Data {
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}