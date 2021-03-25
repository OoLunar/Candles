using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Gtk;

namespace Candles
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Application.Init("Candles", ref args);
            Builder builder = new(File.OpenRead("res/gui.glade"));
            ApplicationWindow applicationWindow = builder.GetObject("window") as ApplicationWindow;
            Reload(builder);
            applicationWindow.ShowAll();
            applicationWindow.DeleteEvent += Exit;
            Application.Run();
        }

        public static void RemoveBirthday(object sender, EventArgs eventArgs) => throw new NotImplementedException();
        public static void Exit(object sender, EventArgs eventArgs)
        {
            // TODO: Save birthdays to birthday file
            Environment.Exit(0);
        }

        public static void Reload(Builder builder)
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
            string birthdayFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "./ForSaken Borders/Candles/birthdays.json");
            if (!File.Exists(birthdayFile))
            {
                // TODO: Make this more efficient. This is ridiculous.
                _ = Directory.CreateDirectory(Directory.GetParent(birthdayFile).FullName);
                File.Create(birthdayFile).Close();
                File.WriteAllText(birthdayFile, "[]");
            }

            Birthday[] birthdays = JsonSerializer.Deserialize<Birthday[]>(File.ReadAllBytes(birthdayFile), new()
            {
                IncludeFields = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            });

            foreach (Birthday birthday in birthdays)
            {
                if (birthday.IsToday())
                {
                    currentBirthdays.Add(ExtensionMethods.CreateRow(birthday.Name));
                }
                else if (birthday.IsUpcoming())
                {
                    upcomingBirthdays.Add(ExtensionMethods.CreateRow($"{birthday.Name} ({birthday.Date.ToString("m", CultureInfo.InvariantCulture)})"));
                }


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
            }

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
        }
    }
}
