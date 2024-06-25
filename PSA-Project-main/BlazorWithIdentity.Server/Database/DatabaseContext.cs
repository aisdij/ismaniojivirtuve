using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database.Tables;

namespace Project.Backend.Server.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<UserInfoTable> UserInfoTable { get; set; }
        public DbSet<ShopItemTable> ShopItemTable { get; set; }
        public DbSet<CartItemTable> CartItemTable { get; set; }
        public DbSet<OrderTable> OrderTable { get; set; }
        public DbSet<ProductTagTable> ProductTagTable { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserInfoTable>(e =>
            {
                e.Property(e => e.Id).IsRequired();
                e.Property(e => e.Email).IsRequired();
                e.Property(e => e.PasswordHashed).IsRequired();
                e.Property(e => e.FirstName).IsRequired();
                e.Property(e => e.LastName).IsRequired();
                e.Property(e => e.Gender);
            });

            builder.Entity<ShopItemTable>(e =>
            {
                e.Property(e => e.Id).IsRequired();
                e.Property(e => e.Name).IsRequired();
                e.Property(e => e.Price).IsRequired();
            });

            builder.Entity<CartItemTable>(e =>
            {
                e.HasKey(e => e.Id);
                e.Property(e => e.Id).IsRequired();
                e.Property(e => e.Count).IsRequired();
                e.Property(e => e.FkShopItemId).IsRequired();
            });

            builder.Entity<OrderTable>(e =>
            {
                e.HasKey(e => e.Id); 
                e.Property(e => e.Id).IsRequired();
                e.Property(e => e.Adress).IsRequired();
                e.Property(e => e.City).IsRequired();
                e.Property(e => e.TotalPrice).IsRequired();
                e.Property(e => e.PostalCode).IsRequired();
                e.Property(e => e.FlatNumber).IsRequired();
                e.Property(e => e.fkOrderStatus)
                    .HasConversion(
                        v => v.ToString(),
                        v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v, true))
                    .IsRequired();
                e.Property(e => e.fkOrderType)
                    .HasConversion(
                        v => v.ToString(),
                        v => (OrderType)Enum.Parse(typeof(OrderType), v, true))
                    .IsRequired();
                e.Property(e => e.OrderDate).IsRequired();
                e.Property(e => e.FkUserId).IsRequired();
            });

            builder.Entity<ProductTagTable>(e =>
            {
                e.HasKey(e => e.FkShopItemId);
                e.Property(e => e.FkShopItemId).IsRequired();
                e.Property(e => e.Name).IsRequired();
            });

            base.OnModelCreating(builder);
        }

    }
}
