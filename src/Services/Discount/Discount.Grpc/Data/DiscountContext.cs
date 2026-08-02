using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
            new Coupon { Id = 1, ProductName = "PlayStation 5", Description = "PS5 Launch Discount", Amount = 200 },
            new Coupon { Id = 2, ProductName = "Xbox Series X", Description = "Xbox Series X Discount", Amount = 150 },
            new Coupon { Id = 3, ProductName = "Nintendo Switch OLED", Description = "Nintendo Switch Discount", Amount = 50 }
        );
    }
}