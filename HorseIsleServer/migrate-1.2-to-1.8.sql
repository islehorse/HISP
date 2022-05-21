## Written by SilicaAndPina,
## This Script is entered into the Public Domain!
## HISP v1.2 to v1.8 migration script...

# To use this script you must find/replace 
# 'master' to your game db name
USE master;

# Add new colums
ALTER TABLE Users ADD COLUMN EmailActivated TEXT(3);

# Initalize new colum data.
UPDATE Users SET EmailActivated="YES";