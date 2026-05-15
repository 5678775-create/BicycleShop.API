namespace BicycleShop.Application.Exceptions;

public class InvalidOrderStatusTransitionException : BusinessRuleValidationException
{
    public InvalidOrderStatusTransitionException(string message) : base(message)
    {
    }
}
