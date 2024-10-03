using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class CurrentLangCurrency
{
    public int Id { get; set; }

    public string CurrentLang { get; set; } = null!;

    public int LangId { get; set; }

    public int CurrencyId { get; set; }

    public string LangTitle { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
