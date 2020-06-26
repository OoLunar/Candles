use chrono::prelude::*;
use dirs;
use notify_rust::Notification;
use std::str;

pub fn get_birthday_list() -> Option<Vec<(String, chrono::Date<Local>)>> {
    let birthday_file = dirs::document_dir().unwrap().join("birthdays.txt");
    let mut birthday_file_contents = String::new();
    let birthday_file_path = birthday_file.clone();
    //Check if file exists, and try to read contents.
    if !birthday_file.exists() {
        //File does not exist, try creating it.
        match std::fs::File::create(birthday_file) {
            Err(ref error) if error.kind() == std::io::ErrorKind::PermissionDenied => println!("Could not create birthday file, Permission Denied, located at: {}", birthday_file_path.to_str().unwrap()),
            Err(ref error) if error.kind() == std::io::ErrorKind::Other => println!("Unexpected error: {}", error),
            Err(error) => println!("Unexpected error: {}", error),
            Ok(_) => (),
        }
        return None;
    } else {
        //File exists, try reading it.
        match std::fs::read_to_string(birthday_file) {
            Err(ref error) if error.kind() == std::io::ErrorKind::PermissionDenied => println!("Could not read birthday file, Permission Denied, located at: {}", birthday_file_path.to_str().unwrap()),
            Err(ref error) if error.kind() == std::io::ErrorKind::Other => println!("Unexpected error: {}", error),
            Err(error) => println!("Unexpected error: {}", error),
            Ok(file_contents) => birthday_file_contents = file_contents.replace("\n\n", "\n"),
        };
    }

    let mut birthdays: Vec<(String, chrono::Date<Local>)> = vec![];
    //Split the file line by line, try parsing.
    let lines: Vec<&str> = birthday_file_contents.split("\n").into_iter().collect();
    for line in lines {
        if line.trim() != "" {
            let mut parts = line.split(|c| matches!(c, ',' | '/')).map(|p| p.trim());
            let person = parts.next().unwrap();
            let month = parts.next().unwrap();
            let day = parts.next().unwrap();
            //Panics upon failure of parsing date instead of returning a `Result` or `Option`.
            let birthday_date = std::panic::catch_unwind(|| {
                return Local.ymd(Local::now().date().year(), month.parse().unwrap(), day.parse().unwrap());
            })
            .expect(format!("{} has an invalid birthdate.", person).as_str());
            birthdays.push((person.to_string(), birthday_date));
        }
    }
    if birthdays.is_empty() {
        return None;
    } else {
        return Some(birthdays);
    }
}

pub fn get_birthdays() -> Option<Vec<String>> {
    let birthdays = get_birthday_list();
    if birthdays.is_some() {
        let mut todays_birthdays: Vec<String> = vec![];
        let current_date = Local::now().date();
        for (person, birthday_date) in birthdays.unwrap() {
            if birthday_date == current_date {
                send_birthday_notif(format!("It's {}'s birthday!", person));
                todays_birthdays.push(person);
            }
        }
        if todays_birthdays.is_empty() {
            return None;
        } else {
            return Some(todays_birthdays);
        }
    } else {
        return None;
    }
}

pub fn get_upcoming_birthdays() -> Option<Vec<(String, chrono::Date<Local>)>> {
    let birthdays = get_birthday_list();
    if birthdays.is_some() {
        let mut upcoming_birthdays: Vec<(String, chrono::Date<Local>)> = vec![];
        let current_date = Local::now().date();
        for (person, birthday_date) in birthdays.unwrap() {
            let birth_day = birthday_date.ordinal0();
            let current_day = current_date.ordinal0();
            if (current_day + 7) >= birth_day && (current_day + 1) <= birth_day {
                send_birthday_notif(format!("{}'s Birthday is coming up on {}", &person, birthday_date.format("%B %e")));
                upcoming_birthdays.push((person, birthday_date));
            }
        }
        if upcoming_birthdays.is_empty() {
            return None;
        } else {
            return Some(upcoming_birthdays);
        }
    } else {
        return None;
    }
}

pub fn send_birthday_notif(summary: String) {
    Notification::new().summary(summary.as_str()).icon("/home/lunar/Pictures/Logos/Candles/vector/isolated-layout.svg").appname("Candles").timeout(0).show().unwrap();
}
