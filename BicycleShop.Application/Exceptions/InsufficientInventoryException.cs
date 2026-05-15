namespace BicycleShop.Application.Exceptions;

public class InsufficientInventoryException : BusinessRuleValidationException
{
    public InsufficientInventoryException(string message) : base(message)
    {
    }
}
