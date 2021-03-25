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
			SaveFile();
			Application.Quit();
		}

		public static void AddBirthday(object sender, EventArgs eventArgs)
		{
			Entry entry = Program.Builder.GetObject("dialog_entry") as Entry;
			if (entry.Text == null) entry.Text = "John Doe";
			Gtk.Calendar calendar = Program.Builder.GetObject("dialog_calendar") as Gtk.Calendar;
			Birthday birthday = new(entry.Text, calendar.Date);
			Program.Birthdays.Add(birthday);
			SaveFile();
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
			SaveFile();
			Program.Reload();
		}

		public static void SortBirthdays()
		{
			ComboBoxText sortingMethod = Program.Builder.GetObject("sorting_methods") as ComboBoxText;
			Program.Birthdays = sortingMethod.ActiveId switch
			{
				// TODO: Try to refrain from creating a new list
				"name" => Program.Birthdays.OrderBy(bday => bday.Name).ToList(),
				"reverse_name" => Program.Birthdays.OrderBy(bday => bday.Name).Reverse().ToList(),
				"date" => Program.Birthdays.OrderBy(bday => bday.Date).ToList(),
				"reverse_date" => Program.Birthdays.OrderBy(bday => bday.Date).Reverse().ToList(),
				_ => Program.Birthdays.OrderBy(bday => bday.Name).ToList()
			};
		}

		public static void SaveFile() => File.WriteAllText(Program.BirthdayFile, JsonSerializer.Serialize(Program.Birthdays));
	}
}
