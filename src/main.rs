extern crate chrono;
extern crate gio;
extern crate gtk;

use chrono::prelude::*;
use dirs;
use gio::prelude::*;
use gtk::prelude::*;
use gtk::{Application, ApplicationWindow};
use notify_rust::Notification;
fn main() {
    let mut birthday_rows: Vec<String> = vec![];
    let birthdays = std::fs::read_to_string(dirs::document_dir().unwrap().join("birthdays.txt")).unwrap();
    let name_and_date: Vec<&str> = birthdays.split("\n").into_iter().collect();
    let current_date = Local::now().date();
    for line in name_and_date {
        let person = line.split(",").into_iter().nth(0).unwrap().trim();
        let month = line.split(",").into_iter().nth(1).unwrap().trim().split("/").into_iter().nth(0).unwrap();
        let day = line.split(",").into_iter().nth(1).unwrap().trim().split("/").into_iter().nth(1).unwrap();
        let birthday_date = std::panic::catch_unwind(|| {
            return Local.ymd(current_date.year(), month.parse().unwrap(), day.parse().unwrap());
        })
        .expect(format!("Person has an invalid date: {}", person).as_str());
        if birthday_date == current_date {
            Notification::new()
                .summary(format!("Candles: It's {}'s birthday!", person).as_str())
                .icon("/usr/share/icons/Humanity/actions/64/help-about.svg")
                .appname("Candles")
                .timeout(0)
                .show()
                .unwrap();
            birthday_rows.push(person.to_string());
        }
        let birthday_iso_week = birthday_date.iso_week().week0();
        let current_iso_week = current_date.iso_week().week0();
        if (current_iso_week + 1) == (birthday_iso_week) {
            let summary = format!("Candles: It's a week before {}'s birthday!", person);
            Notification::new().summary(summary.as_str()).icon("/usr/share/icons/Humanity/actions/64/help-about.svg").appname("Candles").timeout(0).show().unwrap();
        }
    }
    let application = Application::new(Some("net.forsaken-borders.candles"), Default::default()).expect("Failed to initialize GTK application");
    application.connect_activate(move |app| {
        let window = ApplicationWindow::new(app);
        window.set_title("Candles: Birthday Reminder");
        window.set_default_size(600, 400);
        let list = gtk::ListBox::new();
        for birthday_person in &birthday_rows {
            let row = gtk::ListBoxRow::new();
            row.add(&gtk::Label::new(Some(birthday_person.as_str())));
            list.add(&row);
        }
        window.add(&list);
        window.show_all();
    });
    application.run(&[]);
}
