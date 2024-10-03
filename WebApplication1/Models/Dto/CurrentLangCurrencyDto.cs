namespace WebApplication1.Models.Dto;

public class CurrentLangCurrencyDto
{
    public string CurrentLang { get; set; }
    public int LangId { get; set; }
    public int CurrencyId { get; set; }
    public string LangTitle { get; set; }
    public DateTime UpdatedAt { get; set; }
}