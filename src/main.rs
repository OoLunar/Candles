extern crate chrono;
extern crate gio;
extern crate gtk;

use gio::prelude::*;
use gtk::prelude::*;
use gtk::{Application, ApplicationWindow};

mod birthday;

fn main() {
    let application = Application::new(Some("net.forsaken-borders.candles"), Default::default()).expect("Failed to initialize GTK application");
    application.connect_activate(move |app| {
        let window = ApplicationWindow::new(app);
        window.set_title("Candles: Birthday Reminder");
        window.set_default_size(600, 400);
        window.set_icon_from_file("/usr/share/icons/Humanity/actions/64/help-about.svg").unwrap();
        window.set_border_width(10);
        let birthday_list = gtk::ListBox::new();
        for person in birthday::get_birthdays() {
            birthday::send_birthday_notif(format!("It's {} birthday!", person));
            let label = gtk::Label::new(Some(person.as_str()));
            label.set_line_wrap(true);
            let row = gtk::ListBoxRow::new();
            row.set_selectable(false);
            row.set_can_focus(false);
            row.set_property_margin(6);
            row.add(&label);
            birthday_list.add(&row);
        }
        let upcoming_birthday_list = gtk::ListBox::new();
        for (person, birthdate) in birthday::get_upcoming_birthdays() {
            birthday::send_birthday_notif(format!("{}'s Birthday is coming up on {}", person, birthdate.format("%B %e")));
            let label = gtk::Label::new(Some(format!("{} ({})", person, birthdate.format("%B %e")).as_str()));
            label.set_line_wrap(true);
            let row = gtk::ListBoxRow::new();
            row.set_selectable(false);
            row.set_can_focus(false);
            row.set_property_margin(6);
            row.add(&label);
            upcoming_birthday_list.add(&row);
        }
        let main_box = gtk::Box::new(gtk::Orientation::Vertical, 6);
        //create Birthday Label Box
        let birthday_label_box = gtk::Box::new(gtk::Orientation::Horizontal, 0);
        //create Birthday Label
        let birthday_label = gtk::Label::new(None);
        birthday_label.set_markup("<b>Birthdays:</b>");
        birthday_label_box.pack_start(&birthday_label, false, false, 0);
        //create Upcoming Birthday Label Box
        let upcoming_birthdays_label_box = gtk::Box::new(gtk::Orientation::Horizontal, 0);
        //create Upcoming Birthday Label
        let upcoming_birthday_label = gtk::Label::new(None);
        upcoming_birthday_label.set_markup("<b>Upcoming Birthdays:</b>");
        upcoming_birthdays_label_box.pack_start(&upcoming_birthday_label, false, false, 0);
        main_box.add(&birthday_label_box);
        main_box.add(&birthday_list);
        main_box.add(&upcoming_birthdays_label_box);
        main_box.add(&upcoming_birthday_list);
        window.add(&main_box);
        window.show_all();
    });
    application.run(&[]);
}
