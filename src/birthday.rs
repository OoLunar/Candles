use chrono::prelude::*;
use dirs;
use notify_rust::Notification;
use std::str;

fn get_birthday_list() -> Vec<(String, String, String)> {
    let mut birthday_rows: Vec<(String, String, String)> = vec![];
    let birthday_file = std::fs::read_to_string(dirs::document_dir().unwrap().join("birthdays.txt")).unwrap();
    let lines: Vec<&str> = birthday_file.split("\n").into_iter().collect();
    for line in lines {
        let mut parts = line.split(|c| matches!(c, ',' | '/')).map(|p| p.trim());
        birthday_rows.push((parts.next().unwrap().into(), parts.next().unwrap().into(), parts.next().unwrap().into()));
    }
    return birthday_rows;
}

pub fn get_birthdays() -> Vec<String> {
    let mut birthday_rows: Vec<String> = vec![];
    let current_date = Local::now().date();
    for (person, month, day) in get_birthday_list() {
        let birthday_date = std::panic::catch_unwind(|| {
            return Local.ymd(current_date.year(), month.parse().unwrap(), day.parse().unwrap());
        })
        .expect(format!("{} has an invalid birthdate.", person).as_str());
        if birthday_date == current_date {
            birthday_rows.push(person);
        }
    }
    return birthday_rows;
}

pub fn get_upcoming_birthdays() -> Vec<(String, chrono::Date<Local>)> {
    let current_date = Local::now().date();
    let mut upcoming_birthdays: Vec<(String, chrono::Date<Local>)> = vec![];
    for (person, month, day) in get_birthday_list() {
        let birthday_date = std::panic::catch_unwind(|| {
            return Local.ymd(current_date.year(), month.parse().unwrap(), day.parse().unwrap());
        })
        .expect(format!("{} has an invalid birthdate.", person).as_str());
        let birthday_iso_week = birthday_date.ordinal0();
        let current_iso_week = current_date.ordinal0();
        if (current_iso_week + 7) >= birthday_iso_week && (current_iso_week + 1) <= birthday_iso_week {
            upcoming_birthdays.push((person, birthday_date));
        }
    }
    upcoming_birthdays
}

pub fn send_birthday_notif(summary: String) {
    Notification::new().summary(summary.as_str()).icon("/usr/share/icons/Humanity/actions/64/help-about.svg").appname("Candles").timeout(0).show().unwrap();
}
