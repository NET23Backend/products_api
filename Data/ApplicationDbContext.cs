using System;
using Microsoft.EntityFrameworkCore;
using REST_API_Products.Models;

namespace REST_API_Products.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}
		public DbSet<Product> Products { get; set; }
	}
}

