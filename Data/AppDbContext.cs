using BurgerJoint.Models;
using Microsoft.EntityFrameworkCore;

namespace BurgerJoint.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Burger> Burgers => Set<Burger>();

        public DbSet<Order> Orders => Set<Order>();
    }
}