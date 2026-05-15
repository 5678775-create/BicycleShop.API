namespace BicycleShop.Application.DTOs.Errors;

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int StatusCode { get; set; }
}
