using Microsoft.EntityFrameworkCore;
using newspaper.net.Models;

namespace newspaper.net.Data
{
    /// <summary>
    /// Represents the database context for the application.
    /// </summary>
    public class Context : DbContext
	{
		public Context(DbContextOptions<Context> options) : base(options)
		{
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.UseSerialColumns();
		}

		public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
    }
}
