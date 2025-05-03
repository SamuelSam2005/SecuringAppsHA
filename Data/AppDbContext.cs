using Microsoft.EntityFrameworkCore;
using SecureDocumentExchange.Web.Models;

namespace SecureDocumentExchange.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }
}
