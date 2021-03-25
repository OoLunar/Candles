using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using GLib;
using Gtk;

namespace Candles
{
	public class Program
	{
		public static List<Birthday> Birthdays { get; private set; } = new();
		private static readonly Builder builder = new(File.OpenRead("res/gui.glade"));
		private static readonly string birthdayFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "./ForSaken Borders/Candles/birthdays.json");

		public static void Main(string[] args)
		{
			Gtk.Application.Init("Candles", ref args);

			ApplicationWindow applicationWindow = builder.GetObject("window") as ApplicationWindow;
			Button addBirthdayPrompt = builder.GetObject("add_birthday") as Button;
			Button cancelBirthdayPrompt = builder.GetObject("cancel_button") as Button;
			Button addBirthday = builder.GetObject("ok_button") as Button;
			Reload();
			addBirthdayPrompt.Clicked += OpenBirthdayPrompt;
			cancelBirthdayPrompt.Clicked += ClosePromptWindow;
			addBirthday.Clicked += AddBirthday;
			applicationWindow.DeleteEvent += Exit;
			applicationWindow.ShowAll();
			Gtk.Application.Run();
		}

		public static void OpenBirthdayPrompt(object sender, EventArgs eventArgs) => (builder.GetObject("dialog_window") as Dialog).ShowAll();

		public static void ClosePromptWindow(object sender, EventArgs eventArgs)
		{
			(builder.GetObject("dialog_window") as Dialog).Hide();
			Entry entry = builder.GetObject("dialog_entry") as Entry;
			entry.Text = string.Empty;
			Gtk.Calendar calendar = builder.GetObject("dialog_calendar") as Gtk.Calendar;
			calendar.Date = System.DateTime.Now;
		}

		public static void RemoveBirthday(object sender, EventArgs eventArgs)
		{
			Button button = sender as Button;
			Label label = (button.Parent as Box).Children[0] as Label;
			// If there's two of the same names on the list with the same birthday, it'll remove the first one found.
			// TODO: Fix this by assigning each row an id.
			_ = Birthdays.Remove(Birthdays.First(bday => $"{bday.Name} ({bday.Date.ToString("m", CultureInfo.InvariantCulture)})" == label.Text));
			File.WriteAllText(birthdayFile, JsonSerializer.Serialize(Birthdays));
			Reload();
		}

		public static void Exit(object sender, EventArgs eventArgs)
		{
			// TODO: Make this into a stream instead.
			File.WriteAllText(birthdayFile, JsonSerializer.Serialize(Birthdays));
			Gtk.Application.Quit();
			Environment.Exit(0);
		}

		public static void AddBirthday(object sender, EventArgs eventArgs)
		{
			Entry entry = builder.GetObject("dialog_entry") as Entry;
			Gtk.Calendar calendar = builder.GetObject("dialog_calendar") as Gtk.Calendar;
			Birthday birthday = new(entry.Text, calendar.Date);
			// TODO: No need to add to list, find an alternative route.
			// TODO: Make this into a stream instead.
			Birthdays.Add(birthday);
			File.WriteAllText(birthdayFile, JsonSerializer.Serialize(Birthdays));
			ClosePromptWindow(sender, eventArgs);
			Reload();
		}

		public static void Reload()
		{
			// Get birthday lists
			ListBox currentBirthdays = builder.GetObject("today_list") as ListBox;
			ListBox upcomingBirthdays = builder.GetObject("upcoming_list") as ListBox;
			ListBox allBirthdays = builder.GetObject("all_list") as ListBox;
			// Clear all birthdays
			currentBirthdays.Foreach(child => currentBirthdays.Remove(child));
			upcomingBirthdays.Foreach(child => upcomingBirthdays.Remove(child));
			allBirthdays.Foreach(child => allBirthdays.Remove(child));
			// Get birthdays from file
			if (!File.Exists(birthdayFile))
			{
				// TODO: Make this more efficient. This is ridiculous.
				_ = Directory.CreateDirectory(Directory.GetParent(birthdayFile).FullName);
				File.Create(birthdayFile).Close();
				File.WriteAllText(birthdayFile, "[]");
			}

			Birthdays = JsonSerializer.Deserialize<List<Birthday>>(File.ReadAllBytes(birthdayFile), new()
			{
				IncludeFields = true,
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				PropertyNameCaseInsensitive = true
			});

			// Sort birthdays into lists
			foreach (Birthday birthday in Birthdays)
			{
				if (birthday.IsToday())
				{
					currentBirthdays.Add(ExtensionMethods.CreateRow(birthday.Name));
				}
				else if (birthday.IsUpcoming())
				{
					upcomingBirthdays.Add(ExtensionMethods.CreateRow($"{birthday.Name} ({birthday.Date.ToString("m", CultureInfo.InvariantCulture)})"));
				}

				// Add all birthdays to the bottom list, with the option to remove them
				Button button = new("gtk-remove", IconSize.Button);
				button.Halign = Align.End;
				button.FocusOnClick = false;
				button.Clicked += RemoveBirthday;

				Label label = new($"{birthday.Name} ({birthday.Date.ToString("m", CultureInfo.InvariantCulture)})");
				label.LineWrap = true;

				Box box = new(Orientation.Horizontal, 0);
				box.PackStart(label, false, false, 0);
				box.PackEnd(button, false, false, 0);

				ListBoxRow row = new();
				row.Selectable = true;
				row.CanFocus = true;
				row.Margin = 6;
				row.Add(box);
				allBirthdays.Add(row);
			}

			// Add filler rows if no birthdays are coming up or been set
			if (currentBirthdays.Children.Length == 0)
			{
				currentBirthdays.Add(ExtensionMethods.CreateRow("No birthdays today!"));
			}

			if (upcomingBirthdays.Children.Length == 0)
			{
				upcomingBirthdays.Add(ExtensionMethods.CreateRow("No upcoming birthdays!"));
			}

			if (allBirthdays.Children.Length == 0)
			{
				allBirthdays.Add(ExtensionMethods.CreateRow("No birthdays set! Click \"Add\" to create a birthday!"));
			}
			currentBirthdays.ShowAll();
			upcomingBirthdays.ShowAll();
			allBirthdays.ShowAll();
		}
	}
}
