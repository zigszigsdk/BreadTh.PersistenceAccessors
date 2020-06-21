using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BreadTh.PersistenceAccessors.Postgres
{
    public abstract class PostgresDbContextBase : DbContext
    {
        private readonly IConfiguration _configuration;

        protected PostgresDbContextBase(DbContextOptions<PostgresDbContextBase> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("postgres"));
    }
}
