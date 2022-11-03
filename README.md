# HISP - HorseIsleServer Program

[![Build](https://github.com/islehorse/HISP/workflows/build/badge.svg)](https://github.com/islehorse/HISP/actions?query=workflow%3Abuild)

HISP is a "Server Emulator" for Horse Isle 1          
You may know of "private servers" for big MMO games such as **Runescape** or **Club Penguin**          
well they essentailly run off "Server Emulators".          

!!! ALL FEATURES NOW IMPLEMENTED (um, unless theres some obscure thing i dont know about :D)

**tl;dr, think "Club Penguin Rewritten" but with Horse Isle.**


# Installation:

Understand that there are differnet verisons of the HISP package;
HISPd - reimplementation of the Horse Isle 1.0 Server Software
N00BS - a launcher for joining Multiplayer or running a local server for offline singleplayer play
MPN00BS - Same as N00BS but multiplatform

- APT:

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
sudo  bash -c 'echo "deb http://deb.silica.codes debian main">>/etc/apt/sources.list'
sudo apt update
sudo apt install hisp
```

Then simply edit /etc/hisp/server.properties & change to correct database credentials
and start the server using ``systemctl start hisp``

- Manual Install:

If you do not want to use APT, or are on Windows or MacOS, then you can simply download the latest HISPd binary:
here https://islehorse.com/download/

Run it and edit server.properties in the same folder as HISPd

- Websites:

Website is built in PHP 8.0 and based on the original Horse Isle Game Website
requires the following PHP modules to be loaded; "intl", "mysqli" and "mysqlnd" on Ubuntu

you must edit config.php and server.php to configure before it'll work properly.
and is required to sign-up to the private server

the master-site (equivilent to master.horseisle.com) can be found here:
consists of sign up page, and server list, 
https://server.islehorse.com/binaries/download/HISP-Web-Master.zip

and the game-site:
contains the actual game client .SWFs themselves
you must edit config.php
https://server.islehorse.com/binaries/download/HISP-Web-Game.zip

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

# Depends
 HISP Depends on a SQL Server, 
 its been tested and known to work specifically with MariaDB, https://mariadb.org/
 Set its information into server.properties or the server will just crash on first run /-/
 
# Building
 Building the server from source requires Microsoft .NET Core SDK, targetting version 7.0 https://dotnet.microsoft.com/download/dotnet-core
 use ``dotnet build`` to build a debug build, (requires .NET Core Runtime) quickly or one of our publishing XML's
 ex:        
 ``dotnet publish -p:PublishProfile=Linux64.pubxml`` to to build it standalone.
 
 NOTE: At this current time, building projects targeting .NET Core 7.0 inside Visual Studio requires Visual Studio 2022 PREVIEW and does not work in the stable build
 
 
 # Credits
 
Li (They/Them)

Supertiger (He/Him)

Olebeck (They/Them)
 
