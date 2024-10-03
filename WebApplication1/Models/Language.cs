using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Language
{
    public int Id { get; set; }

    public string LangCode { get; set; } = null!;

    public string LangName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
