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

		public Birthday(string name, DateTime date)
		{
			Name = name;
			Date = date;
		}

		public bool IsToday() => Date.DayOfYear == DateTime.Now.DayOfYear;
		public bool IsUpcoming()
		{
			DateTime now = DateTime.Now;
			return Date.DayOfYear < now.AddDays(14).DayOfYear && Date.DayOfYear >= now.DayOfYear;
		}
	}
}
