using Newtonsoft.Json;

namespace WebApplication1.Models
{
    public class CoinDeskResponse
    {
        public TimeInfo Time { get; set; }

        public string Disclaimer { get; set; }

        public string ChartName { get; set; }

        public Dictionary<string, CurrencyInfo> Bpi { get; set; }
    }

    public class TimeInfo
    {
        public string Updated { get; set; }

        public string UpdatedISO { get; set; }

        public string UpdatedUK { get; set; }
    }

    public class CurrencyInfo
    {
        public string Code { get; set; }

        public string Symbol { get; set; }

        public decimal Rate { get; set; }

        public string Description { get; set; }
        
        [JsonProperty("rate_float")]
        public decimal RateFloat { get; set; }
    }
}