namespace WebApplication1.Models.Dto;

public class CurrencyInfoDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }
    public decimal Rate { get; set; }
    public decimal RateFloat { get; set; }
    public string? LangTitle { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}