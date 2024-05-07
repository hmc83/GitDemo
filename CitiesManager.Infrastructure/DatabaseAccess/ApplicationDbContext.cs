using CitiesManager.Core.Identity;
using CitiesManager.Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.Infrastructure.DatabaseAccess
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<City> Cities { get; set; }
        public ApplicationDbContext(DbContextOptions options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);
            City dummy = new City() { CityID = Guid.Parse("f3a1565c-bb82-465c-b6a5-8d327fbeb2a6"), CityName = "dummy" };
            modelbuilder.Entity<City>().HasData(dummy);
        }
    }
}
