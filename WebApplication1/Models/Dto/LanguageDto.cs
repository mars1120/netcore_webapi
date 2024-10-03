namespace WebApplication1.Models.Dto;

public class LanguageDto
{
    public int Id { get; set; }
    
    public string LangCode { get; set; } = null!;

    public string LangName { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}