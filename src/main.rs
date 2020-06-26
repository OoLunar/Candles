extern crate chrono;
extern crate gio;
extern crate gtk;

use gio::prelude::*;
use gtk::prelude::*;
use gtk::{Application, ApplicationWindow};

mod functions;

fn main() {
    let application = Application::new(Some("net.forsaken-borders.candles"), Default::default()).expect("Failed to initialize GTK application");
    application.connect_activate(move |app| {
        let window = ApplicationWindow::new(app);
        let scroll_window = gtk::ScrolledWindow::new::<gtk::Adjustment, gtk::Adjustment>(None, None);
        let main_box = gtk::Box::new(gtk::Orientation::Vertical, 6);
        let todays_birthdays = functions::get_birthdays();
        let todays_label_box = gtk::Box::new(gtk::Orientation::Horizontal, 0);
        let todays_label = gtk::Label::new(None);
        let todays_list = gtk::ListBox::new();
        let upcoming_birthdays = functions::get_upcoming_birthdays();
        let upcoming_label_box = gtk::Box::new(gtk::Orientation::Horizontal, 0);
        let upcoming_label = gtk::Label::new(None);
        let upcoming_list = gtk::ListBox::new();
        let all_birthdays = functions::get_birthday_list();
        let all_label_box = gtk::Box::new(gtk::Orientation::Horizontal, 0);
        let all_label = gtk::Label::new(None);
        let all_list = gtk::ListBox::new();
        if todays_birthdays.is_some() {
            for person in todays_birthdays.unwrap() {
                let label = gtk::Label::new(Some(person.as_str()));
                let row = gtk::ListBoxRow::new();
                label.set_line_wrap(true);
                row.set_selectable(false);
                row.set_can_focus(false);
                row.set_property_margin(6);
                row.add(&label);
                todays_list.add(&row);
            }
        } else {
            let label = gtk::Label::new(Some("No birthdays today!"));
            let row = gtk::ListBoxRow::new();
            label.set_line_wrap(true);
            row.set_selectable(false);
            row.set_can_focus(false);
            row.set_property_margin(6);
            row.add(&label);
            todays_list.add(&row);
        };
        if upcoming_birthdays.is_some() {
            for (person, birthdate) in upcoming_birthdays.unwrap() {
                let label = gtk::Label::new(Some(format!("{} ({})", person, birthdate.format("%B %e")).as_str()));
                let row = gtk::ListBoxRow::new();
                label.set_line_wrap(true);
                row.set_selectable(false);
                row.set_can_focus(false);
                row.set_property_margin(6);
                row.add(&label);
                upcoming_list.add(&row);
            }
        } else {
            let label = gtk::Label::new(Some("No upcoming birthdays!"));
            let row = gtk::ListBoxRow::new();
            label.set_line_wrap(true);
            row.set_selectable(false);
            row.set_can_focus(false);
            row.set_property_margin(6);
            row.add(&label);
            upcoming_list.add(&row);
        };
        if all_birthdays.is_some() {
            for (person, birthdate) in all_birthdays.unwrap() {
                let label = gtk::Label::new(Some(format!("{} ({})", person, birthdate.format("%B %e")).as_str()));
                let row = gtk::ListBoxRow::new();
                label.set_line_wrap(true);
                row.set_selectable(false);
                row.set_can_focus(false);
                row.set_property_margin(6);
                row.add(&label);
                all_list.add(&row);
            }
        } else {
            let label = gtk::Label::new(Some("No birthdays registered... Click the plus button to add some!"));
            let row = gtk::ListBoxRow::new();
            label.set_line_wrap(true);
            row.set_selectable(false);
            row.set_can_focus(false);
            row.set_property_margin(6);
            row.add(&label);
            all_list.add(&row);
        };
        todays_label.set_markup("<b>Birthdays:</b>");
        todays_label_box.pack_start(&todays_label, false, false, 0);
        upcoming_label.set_markup("<b>Upcoming Birthdays:</b>");
        upcoming_label_box.pack_start(&upcoming_label, false, false, 0);
        all_label.set_markup("<b>All birthdays:</b>");
        all_label_box.pack_start(&all_label, false, false, 0);
        scroll_window.set_min_content_height(410);
        main_box.add(&todays_label_box);
        main_box.add(&todays_list);
        main_box.add(&upcoming_label_box);
        main_box.add(&upcoming_list);
        main_box.add(&all_label_box);
        main_box.add(&all_list);
        scroll_window.add(&main_box);
        window.add(&scroll_window);
        window.set_title("Candles: Birthday Reminder");
        window.set_default_size(600, 400);
        window.set_icon_from_file("/home/lunar/Pictures/Logos/Candles/vector/isolated-layout.svg").unwrap();
        window.set_border_width(10);
        window.show_all();
    });
    application.run(&[]);
}
