using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ShareNet.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Models.File> Files { get; set; }
        public DbSet<Models.ShareNetLog> Logi { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
