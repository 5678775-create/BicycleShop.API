using BicycleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Data.Context;

public class BicycleShopDbContext : DbContext
{
    public BicycleShopDbContext(DbContextOptions<BicycleShopDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
}