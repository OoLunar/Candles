using System;
using System.Text.Json.Serialization;

namespace Candles
{
    public class Birthday
    {
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; private set; }

        public bool IsToday() => Date == DateTime.Now;
        public bool IsUpcoming()
        {
            DateTime now = DateTime.Now;
            return Date < now.AddDays(14) && Date >= now;
        }
    }
}