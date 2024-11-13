using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Models;

namespace OnlineShop_API.Data
{
    public class OnlineShopDbContext : DbContext
    {
        public OnlineShopDbContext(DbContextOptions<OnlineShopDbContext> options) : base(options)
        {
        }

        // DbSets for all the models
        public DbSet<Categories> Categories { get; set; }
        public DbSet<SubCategories> SubCategories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<JWT> JWTs { get; set; }
        public object JWT { get; internal set; }
    }
}
