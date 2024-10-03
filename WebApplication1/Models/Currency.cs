using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Currency
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public decimal Rate { get; set; }

    public string Description { get; set; } = null!;

    public decimal RateFloat { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
