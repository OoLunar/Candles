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
		/// <summary>
		/// Creates a <see cref="ListBoxRow"/> containing a <see cref="Label"/>.
		/// </summary>
		/// <param name="text">The text to put into the <see cref="Label"/>.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Saves <see cref="Program.Birthdays"/> into a JSON file, then exits.
		/// </summary>
		/// <param name="_sender">Unused.</param>
		/// <param name="_eventArgs">Unused.</param>
		public static void Exit(object _sender, EventArgs _eventArgs)
		{
			SaveFile();
			Application.Quit();
		}

		/// <summary>
		/// Adds a new <see cref="Birthday"/> to <see cref="Program.Birthdays"/> by getting the values from the "Create Birthday Dialog" window.
		/// </summary>
		/// <param name="_sender">Unused.</param>
		/// <param name="_eventArgs">Unused.</param>
		public static void AddBirthday(object _sender, EventArgs _eventArgs)
		{
			Entry entry = Program.Builder.GetObject("dialog_entry") as Entry;
			if (entry.Text == null) entry.Text = "John Doe";
			Gtk.Calendar calendar = Program.Builder.GetObject("dialog_calendar") as Gtk.Calendar;
			Birthday birthday = new(entry.Text, calendar.Date);
			Program.Birthdays.Add(birthday);
			SaveFile();
			Program.ClosePromptWindow(_sender, _eventArgs);
			Program.Reload();
		}

		/// <summary>
		/// Removes a <see cref="Birthday"/> from <see cref="Program.Birthdays"/> by clicking on the "X" <see cref="Button"/> in the "All List" <see cref="ListBoxRow"/>.
		/// </summary>
		/// <param name="sender">The <see cref="Button"/> that's attached to the <see cref="ListBoxRow"/> that's to be removed.</param>
		/// <param name="_eventArgs">Unused.</param>
		public static void RemoveBirthday(object sender, EventArgs _eventArgs)
		{
			Button button = sender as Button;
			Label label = (button.Parent as Box).Children[0] as Label;
			// If two people have the exact same name and the same birthday, it'll remove the first one found.
			// Unsure if this is an issue, or if it'll even affect the user.
			// TODO: Find a better/more accurate way to compare the rows. Attaching a hidden id to a row seems to be the best option.
			_ = Program.Birthdays.Remove(Program.Birthdays.First(bday => $"{bday.Name} ({bday.Date.ToString("m", CultureInfo.InvariantCulture)})" == label.Text));
			SaveFile();
			Program.Reload();
		}

		/// <summary>
		/// Sorts <see cref="Program.Birthdays"/> by getting the current value from the "sorting_methods" <see cref="ComboTextBox"/>.
		/// </summary>
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

		/// <summary>
		/// Serializes <see cref="Program.Birthdays"/> to the <see cref="Program.BirthdayFile"/>.
		/// </summary>
		public static void SaveFile() => File.WriteAllText(Program.BirthdayFile, JsonSerializer.Serialize(Program.Birthdays));
	}
}
