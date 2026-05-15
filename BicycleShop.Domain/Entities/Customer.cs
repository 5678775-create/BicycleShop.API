namespace BicycleShop.Domain.Entities;

using BicycleShop.Domain.Enums;

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerLoyaltyTier LoyaltyTier { get; set; } = CustomerLoyaltyTier.Bronze;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
