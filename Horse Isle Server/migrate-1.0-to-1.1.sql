## Written by SilicaAndPina,
## This Script is entered into the Public Domain!
## HISP v1.0 to v1.1 migration script...

# To use this script you must find/replace 
# 'master' to your master db name
# and 'beta' to your game db name
CREATE DATABASE IF NOT EXISTS master;
CREATE TABLE IF NOT EXISTS master.Users(Id INT, Username TEXT(16),Email TEXT(128),Country TEXT(128),SecurityQuestion Text(128),SecurityAnswerHash TEXT(128),Age INT,PassHash TEXT(128), Salt TEXT(128),Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3));
# Transfer user table to master db
USE beta;
INSERT INTO master.Users(SELECT Id,Username,Email,Country,SecurityQuestion,SecurityAnswerHash,Age,PassHash,Salt,Gender,Admin,Moderator FROM Users);
ALTER TABLE Users DROP COLUMN Email;
ALTER TABLE Users DROP COLUMN Country;
ALTER TABLE Users DROP COLUMN SecurityQuestion;
ALTER TABLE Users DROP COLUMN SecurityAnswerHash;
ALTER TABLE Users DROP COLUMN Age;

# Add new colums
ALTER TABLE UserExt ADD COLUMN TotalLogins INT;
UPDATE UserExt SET TotalLogins=0;
DROP TABLE OnlineUsers; # Server will re-generate the table when next started

# Change table sizes
ALTER TABLE UserExt CHANGE COLUMN ProfilePage ProfilePage TEXT(4000);
ALTER TABLE UserExt CHANGE COLUMN PrivateNotes PrivateNotes TEXT(65535);
ALTER TABLE MailBox CHANGE COLUMN Subject Subject TEXT(100);
ALTER TABLE MailBox CHANGE COLUMN Message Message TEXT(65535);
ALTER TABLE Horses CHANGE COLUMN description description TEXT(4000);
ALTER TABLE WildHorse CHANGE COLUMN description description TEXT(4000);
ALTER TABLE Ranches CHANGE COLUMN title title TEXT(50);
ALTER TABLE Ranches CHANGE COLUMN description description TEXT(250);
