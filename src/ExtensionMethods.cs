using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Gtk;

namespace Candles
{
	public static class ExtensionMethods
	{
		public static ListBoxRow CreateRow(string text)
		{
			Label label = new(text);
			label.LineWrap = true;

			ListBoxRow row = new();
			row.Selectable = true;
			row.CanFocus = true;
			row.Margin = 6;
			row.Add(label);

			return row;
		}

		public static void Exit(object sender, EventArgs eventArgs)
		{
			File.WriteAllText(Program.BirthdayFile, JsonSerializer.Serialize(Program.Birthdays));
			Application.Quit();
		}

		public static void AddBirthday(object sender, EventArgs eventArgs)
		{
			Entry entry = Program.Builder.GetObject("dialog_entry") as Entry;
			Gtk.Calendar calendar = Program.Builder.GetObject("dialog_calendar") as Gtk.Calendar;
			Birthday birthday = new(entry.Text, calendar.Date);
			// TODO: Shouldn't need to add to list, try to find an alternative route.
			Program.Birthdays.Add(birthday);
			File.WriteAllText(Program.BirthdayFile, JsonSerializer.Serialize(Program.Birthdays));
			Program.ClosePromptWindow(sender, eventArgs);
			Program.Reload();
		}

		public static void RemoveBirthday(object sender, EventArgs eventArgs)
		{
			Button button = sender as Button;
			Label label = (button.Parent as Box).Children[0] as Label;
			// If two people have the exact same name and the same birthday, it'll remove the first one found.
			// Unsure if this is an issue, or if it'll even affect the user.
			// TODO: Find a better/more accurate way to compare the rows.
			_ = Program.Birthdays.Remove(Program.Birthdays.First(bday => $"{bday.Name} ({bday.Date.ToString("m", CultureInfo.InvariantCulture)})" == label.Text));
			File.WriteAllText(Program.BirthdayFile, JsonSerializer.Serialize(Program.Birthdays));
			Program.Reload();
		}
	}
}
