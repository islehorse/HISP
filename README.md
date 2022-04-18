# HISP - HorseIsleServer Program



[![Build](https://github.com/islehorse/HISP/workflows/build/badge.svg)](https://github.com/islehorse/HISP/actions?query=workflow%3Abuild)

HISP is a "Server Emulator" for Horse Isle 1          
You may know of "private servers" for big MMO games such as **Runescape** or **Club Penguin**          
well they essentailly run off "Server Emulators".          

!!! ALL FEATURES NOW IMPLEMENTED (um, unless theres some obscure thing i dont know about :D)

**tl;dr, think "Club Penguin Rewritten" but with Horse Isle.**

# Installation:
Install on Ubuntu via APT:
```
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 34F644BC70C746CE48139C595129317F33AE659C
sudo add-apt-repository 'deb http://deb.silica.codes debian main'
sudo apt update
sudo apt install hisp
```

Install on Debian via APT:
```
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 34F644BC70C746CE48139C595129317F33AE659C
sudo  bash -c 'echo 'deb http://deb.silica.codes debian main'>>/etc/apt/sources.list'
sudo apt update
sudo apt install hisp
```

# Commands     
(legend: <> Required, [] Optional)
```
== Admin Commands ==            
    %GIVE                 
              OBJECT <itemid / RANDOM> [username / ALL]         
              MONEY <amount> [username]             
              HORSE <breedid> [username]         
              QUEST <questid> [FORCE]       
              AWARD <awardid> [username]
    %GOTO                             
              <x>,<y>         
              PLAYER <playername>        
              AREA <locationname>           
              NPC <npcname>             
    %SWF <swf> [username / ALL]
    %DELITEM <itemid> [username]
    %JUMP <playername> HERE              
    %NOCLIP (toggle)            
    %MODHORSE <id> <stat> <value>
    %CALL HORSE            
    %SHUTDOWN
== Moderator Commands ==         
    %KICK <username> [reason]         
    %RULES <username>          
    %STEALTH (toggle)        
    %BAN <username> [reason]     
    %PRISON <username>
    %UNBAN <username>       
    %ESCAPE               
== Player Commands ==         
    !VERSION 
    !MUTE                    
              ALL        
              GLOBAL       
              ISLAND        
              NEAR        
              HERE        
              BUDDY        
              PM        
              BR          
              SOCIALS         
              LOGINS            
    !UNMUTE              
              ALL         
              GLOBAL         
              ISLAND         
              NEAR        
              HERE         
              BUDDY       
              PM           
              BR             
              SOCIALS           
              LOGINS               
    !HEAR (same as !UNMUTE)            
    !AUTOREPLY [message]              
    !QUIZ                   
    !WARP           
              <playername>           
              <locationame>        
    !DANCE <udlr>       
```

# Noobs Package (For people who go WTF IS A SQL ?)
- Download HISP-N00BS zip
- Open the HISP-N00BS executable
- Within a few secs should now be playing Horse Isle.

# Normal Setup (I want to actually run a server).      
- Download the latest Windows or Linux binary.        
- Setup a SQL Server, (eg, MariaDB).         
- Setup a webserver with PHP8.0, intl and mysqli modules (eg, Apache).       
- Create a database for the master site, and for all game sites you may have  
- Copy files from the binary ZIP's www/master-site into your webserver.        
  have a separate virtual host for each www/game-site.     
- Edit each game-site/config.php to have your SQL login information.         
  Change the server host to your public IP or a Domain that points.       
  to it and also change the HMAC Secret       
- Edit master-site/servers.php and put each server your hosting            
  in the array. this is what will appear in the server list                 
- Edit master-site/config.php and set your SQL Server credentials, for the master-site             
  and change the hmac_secret to match that of every game-site        
- For each server, run the HorseIsleServer binary. on first run, it will         
  crash due to trying to connect. But will create a "server.properties" file,           
  as well as a CrossDomainPolicy.xml, Edit server.properies and change the DB connection.      
  to your SQL server credentials- Change whatever other settings you want there as well.       
- Run HorseIsleServer again and this time it will connect to the server and start up.      
- Create an account on the master-site/ webserver.       
  And login using game-site/Horseisle.php,
- That's it your now running HISP Server.       
  Forward the port you used for the server and 80 for the webserver.        
  And people can login over the internet.         

- You can give yourself admin by executing ``UPDATE Users SET Admin='YES' Moderator='YES' WHERE Username='<YOUR USERNAME>'``
  on the master database, (and any game databases)
  
# Depends
 HISP Depends on a SQL Server, 
 its been tested and known to work specifically with MariaDB, https://mariadb.org/
 Set its information into server.properties or the server will just crash on first run /-/
 
# Building
 Building the server from source requires Microsoft .NET Core SDK, targetting version 5.0 https://dotnet.microsoft.com/download/dotnet-core
 use ``dotnet build`` to build a debug build, (requires .NET Core Runtime) quickly or one of our publishing XML's
 ex:        
 ``dotnet publish -p:PublishProfile=Linux64.pubxml`` to to build it standalone.
 
# Web Server
 Theres a bunch of saved files from the Horse Isle 1.0 website, as well as some basic functionality to make it work
 like the original site, the files are in the "HorseIsleWeb" folder, it requires PHP 8.0, with the modules "intl" and "mysqli"
