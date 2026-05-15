using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Data.Context;

public class BicycleShopDbContext : DbContext
{
    private static readonly Guid MountainBikesCatalogId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CityBikesCatalogId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid EBikesCatalogId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AccessoriesCatalogId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static readonly Guid MountainBikeProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid CityBikeProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
    private static readonly Guid ElectricBikeProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
    private static readonly Guid HelmetProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
    private static readonly Guid BikeLightProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5");
    private static readonly Guid GlovesProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6");

    public BicycleShopDbContext(DbContextOptions<BicycleShopDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<AppliedPromotion> AppliedPromotions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCatalog(modelBuilder);
        ConfigureCustomer(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureInventoryReservation(modelBuilder);
        ConfigurePromotion(modelBuilder);
        ConfigureAppliedPromotion(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Catalog>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Catalog)
            .HasForeignKey(p => p.CatalogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Catalog>()
            .HasMany(c => c.Promotions)
            .WithOne(p => p.Catalog)
            .HasForeignKey(p => p.CatalogId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .Property(c => c.LoyaltyTier)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(CustomerLoyaltyTier.Bronze);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Inventory)
            .WithOne(i => i.Product)
            .HasForeignKey<Inventory>(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(OrderStatus.Pending);

        modelBuilder.Entity<Order>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.LoyaltyDiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.BulkDiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.PromotionDiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalDiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.AppliedPromotions)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Reservations)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.LineSubtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.LineTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.ProductId)
            .IsUnique();

        modelBuilder.Entity<Inventory>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("CK_Inventories_QuantityAvailable_NonNegative", "\"QuantityAvailable\" >= 0");
                t.HasCheckConstraint("CK_Inventories_QuantityReserved_NonNegative", "\"QuantityReserved\" >= 0");
                t.HasCheckConstraint("CK_Inventories_Reserved_NotGreaterThanAvailable", "\"QuantityReserved\" <= \"QuantityAvailable\"");
            });
    }

    private static void ConfigureInventoryReservation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryReservation>()
            .HasOne(r => r.Product)
            .WithMany(p => p.InventoryReservations)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePromotion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Promotion>()
            .Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(32);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.DiscountPercent)
            .HasPrecision(18, 2);
    }

    private static void ConfigureAppliedPromotion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppliedPromotion>()
            .Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AppliedPromotion>()
            .HasOne(p => p.Promotion)
            .WithMany(p => p.AppliedPromotions)
            .HasForeignKey(p => p.PromotionId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedUpdatedAt = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);
        var activeFrom = new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc);
        var activeTo = new DateTime(2026, 6, 14, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Catalog>().HasData(
            new Catalog { Id = MountainBikesCatalogId, Name = "Mountain Bikes", Description = "Bicycles for trails and off-road riding." },
            new Catalog { Id = CityBikesCatalogId, Name = "City Bikes", Description = "Comfortable bicycles for commuting." },
            new Catalog { Id = EBikesCatalogId, Name = "E-Bikes", Description = "Electric bicycles with assisted pedaling." },
            new Catalog { Id = AccessoriesCatalogId, Name = "Accessories", Description = "Helmets, lights, gloves and other cycling accessories." });

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), FirstName = "Bronze", LastName = "Customer", Email = "bronze.customer@example.com", LoyaltyTier = CustomerLoyaltyTier.Bronze },
            new Customer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), FirstName = "Silver", LastName = "Customer", Email = "silver.customer@example.com", LoyaltyTier = CustomerLoyaltyTier.Silver },
            new Customer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"), FirstName = "Gold", LastName = "Customer", Email = "gold.customer@example.com", LoyaltyTier = CustomerLoyaltyTier.Gold });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = MountainBikeProductId, Name = "Mountain Bike", Description = "Durable mountain bike for rough terrain.", Price = 32000m, StockQuantity = 12, CatalogId = MountainBikesCatalogId },
            new Product { Id = CityBikeProductId, Name = "City Bike", Description = "Lightweight city bike for daily rides.", Price = 18000m, StockQuantity = 15, CatalogId = CityBikesCatalogId },
            new Product { Id = ElectricBikeProductId, Name = "Electric Bike", Description = "Electric bike with long-range battery.", Price = 62000m, StockQuantity = 6, CatalogId = EBikesCatalogId },
            new Product { Id = HelmetProductId, Name = "Helmet", Description = "Protective cycling helmet.", Price = 1800m, StockQuantity = 40, CatalogId = AccessoriesCatalogId },
            new Product { Id = BikeLightProductId, Name = "Bike Light", Description = "Rechargeable front bike light.", Price = 900m, StockQuantity = 55, CatalogId = AccessoriesCatalogId },
            new Product { Id = GlovesProductId, Name = "Gloves", Description = "Comfortable cycling gloves.", Price = 650m, StockQuantity = 60, CatalogId = AccessoriesCatalogId });

        modelBuilder.Entity<Inventory>().HasData(
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"), ProductId = MountainBikeProductId, QuantityAvailable = 12, QuantityReserved = 0, UpdatedAt = seedUpdatedAt },
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2"), ProductId = CityBikeProductId, QuantityAvailable = 15, QuantityReserved = 0, UpdatedAt = seedUpdatedAt },
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3"), ProductId = ElectricBikeProductId, QuantityAvailable = 6, QuantityReserved = 0, UpdatedAt = seedUpdatedAt },
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc4"), ProductId = HelmetProductId, QuantityAvailable = 40, QuantityReserved = 0, UpdatedAt = seedUpdatedAt },
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc5"), ProductId = BikeLightProductId, QuantityAvailable = 55, QuantityReserved = 0, UpdatedAt = seedUpdatedAt },
            new Inventory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc6"), ProductId = GlovesProductId, QuantityAvailable = 60, QuantityReserved = 0, UpdatedAt = seedUpdatedAt });

        modelBuilder.Entity<Promotion>().HasData(
            new Promotion
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1"),
                Name = "Spring Bike Sale",
                Description = "Seasonal promotion for all active orders.",
                Type = PromotionType.TimeBased,
                DiscountPercent = 10m,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                IsActive = true,
                CreatedAt = seedUpdatedAt
            },
            new Promotion
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd2"),
                Name = "Accessories Promo",
                Description = "Discount for cycling accessories.",
                Type = PromotionType.CategoryBased,
                DiscountPercent = 15m,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                CatalogId = AccessoriesCatalogId,
                IsActive = true,
                CreatedAt = seedUpdatedAt
            });
    }
}
