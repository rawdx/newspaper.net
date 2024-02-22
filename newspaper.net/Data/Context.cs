using Microsoft.EntityFrameworkCore;
using newspaper.net.Models;

namespace newspaper.net.Data
{
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
	}
}
