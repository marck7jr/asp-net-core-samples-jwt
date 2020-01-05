using AspNetCoreSamplesJwt.Models;
using AspNetCoreSamplesJwt.Services;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreSamplesJwt.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		public DbSet<UserAccount> UserAccounts { get; set; }
		public DbSet<JwtRefreshTokenData> JwtRefreshTokens { get; set; }
	}
}