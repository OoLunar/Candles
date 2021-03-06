﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Gtk;

namespace Candles
{
	public class Program
	{
		/// <summary>
		/// Holds all the birthdays.
		/// </summary>
		public static List<Birthday> Birthdays { get; internal set; } = new();

		/// <summary>
		/// Creates the GUI and gets objects from the GUI.
		/// </summary>
		public static readonly Builder Builder = new(File.OpenRead("res/gui.glade"));

		/// <summary>
		/// The path to the birthday JSON file.
		/// </summary>
		public static readonly string BirthdayFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "./ForSaken Borders/Candles/birthdays.json");

		// TODO: Additionally create command line args that allows to get a list of whose birthdays it is
		// TODO: Export the birthdays into a variety of formats. Yaml, SQLite, CSV, Excel, etc
		// TODO: Error proof your application as much as possible.
		// TODO: Application settings. Default sort option, birthday format, light & dark mode, *maybe* change application layout?
		// TODO: Documentation
		public static void Main(string[] args)
		{
			Application.Init("Candles", ref args);
			ApplicationWindow applicationWindow = Builder.GetObject("window") as ApplicationWindow;
			Button addBirthdayPrompt = Builder.GetObject("add_birthday") as Button;
			Button cancelBirthdayPrompt = Builder.GetObject("cancel_button") as Button;
			Button addBirthday = Builder.GetObject("ok_button") as Button;
			ComboBoxText sortingMethod = Builder.GetObject("sorting_methods") as ComboBoxText;
			Reload();
			addBirthdayPrompt.Clicked += OpenBirthdayPrompt;
			cancelBirthdayPrompt.Clicked += ClosePromptWindow;
			addBirthday.Clicked += ExtensionMethods.AddBirthday;
			sortingMethod.Changed += (sender, eventArgs) => Reload();
			applicationWindow.DeleteEvent += ExtensionMethods.Exit;
			applicationWindow.ShowAll();
			Application.Run();
		}

		/// <summary>
		/// Opens the dialog window to add a birthday.
		/// </summary>
		/// <param name="_sender">Unused.</param>
		/// <param name="_eventArgs">Unused.</param>
		public static void OpenBirthdayPrompt(object _sender, EventArgs _eventArgs) => (Builder.GetObject("dialog_window") as Dialog).ShowAll();

		/// <summary>
		/// Cancels adding a birthday and closes the dialog window. Resets the calendar to local time and resets the name to John Doe.
		/// </summary>
		/// <param name="_sender">Unused.</param>
		/// <param name="_eventArgs">Unused.</param>
		public static void ClosePromptWindow(object _sender, EventArgs _eventArgs)
		{
			(Builder.GetObject("dialog_window") as Dialog).Hide();
			Entry entry = Builder.GetObject("dialog_entry") as Entry;
			entry.Text = string.Empty;
			Gtk.Calendar calendar = Builder.GetObject("dialog_calendar") as Gtk.Calendar;
			calendar.Date = DateTime.Now;
		}

		/// <summary>
		/// Reads the birthdays from <see cref="BirthdayFile"/> and sorts them into <see cref="Birthdays"/>.
		/// </summary>
		public static void Reload()
		{
			// Get birthday lists
			ListBox currentBirthdays = Builder.GetObject("today_list") as ListBox;
			ListBox upcomingBirthdays = Builder.GetObject("upcoming_list") as ListBox;
			ListBox allBirthdays = Builder.GetObject("all_list") as ListBox;

			// Clear all birthdays
			currentBirthdays.Foreach(child => currentBirthdays.Remove(child));
			upcomingBirthdays.Foreach(child => upcomingBirthdays.Remove(child));
			allBirthdays.Foreach(child => allBirthdays.Remove(child));

			// Get birthdays from file
			string birthdayFileDir = Path.GetDirectoryName(BirthdayFile);
			// TODO: Could be combined into one if statement?
			if (!Directory.Exists(birthdayFileDir)) _ = Directory.CreateDirectory(birthdayFileDir);
			if (!File.Exists(BirthdayFile))
			{
				FileStream openFile = File.Create(BirthdayFile);
				openFile.Write(Encoding.UTF8.GetBytes("[]"));
				openFile.Close();
			}

			// TODO: Read from stream instead of reading it all into memory. Can be done by using the async version
			Birthdays = JsonSerializer.Deserialize<List<Birthday>>(File.ReadAllBytes(BirthdayFile), new()
			{
				IncludeFields = true,
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				PropertyNameCaseInsensitive = true
			});

			ExtensionMethods.SortBirthdays();
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
				button.Clicked += ExtensionMethods.RemoveBirthday;

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
