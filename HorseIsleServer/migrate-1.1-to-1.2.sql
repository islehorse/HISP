## Written by SilicaAndPina,
## This Script is entered into the Public Domain!
## HISP v1.1 to v1.2 migration script...

# To use this script you must find/replace 
# 'game1' to your game db name
USE game1;

# Add new colums
ALTER TABLE ShopInventory ADD COLUMN Data INT;

# Initalize new colum data.
UPDATE ShopInventory SET Data=0;