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

N00BS - a launcher for joining Multiplayer or running a local server for offline singleplayer play (deprecated)

MPN00BS - Same as N00BS but multiplatform

Game relies on a SQL Server, any should work, but i have only tested it with MariaDB, 
(and i guess SQLLite for the one used in the launcher version)

- Docker

```
git clone https://github.com/islehorse/HISP.git --recursive
cd HISP
docker compose up -d
```
default docker compose will give you: 

master-site: localhost:12323
game-site: localhost:12322
game-server: localhost:12321

configuration files at /etc/hisp 
after that you may need to change certain domains at:

```
/etc/hisp/web.cfg 
/etc/hisp/game1.cfg 
/etc/hisp/servers.json
```

- APT:

Install on Ubuntu or Debian via APT:
```
sudo curl https://silica.codes/api/packages/islehorse/debian/repository.key -o /etc/apt/keyrings/forgejo-islehorse.asc
echo "deb [signed-by=/etc/apt/keyrings/forgejo-islehorse.asc] https://silica.codes/api/packages/islehorse/debian debian main" | sudo tee -a /etc/apt/sources.list.d/hisp.list
sudo apt update
```

Then simply edit /etc/hisp/server.properties & change to correct database credentials
and start the server using ``systemctl start hisp``

- Manual Install:

If you do not want to use APT, or are on Windows or MacOS, then you can simply download the latest HISPd binary:
here https://github.com/islehorse/HISP/actions/

Run it and edit server.properties in the same folder as HISPd

- Websites:

Website is built in PHP 8.0 and based on the original Horse Isle Game Website
requires the following PHP modules to be loaded; "intl", "mysqli" and "mysqlnd" on Ubuntu

you must edit config.php and server.php to configure before it'll work properly.
and is required to sign-up to the private server

the master-site (equivilent to master.horseisle.com) can be found here:
consists of sign up page, and server list, 

https://github.com/islehorse/HISP/actions/

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
 
# Building
 Building the server from source requires Microsoft .NET Core SDK, targetting version 9.0 https://dotnet.microsoft.com/download/dotnet-core
 use ``dotnet build`` to build a debug build, (requires .NET Core Runtime) quickly or one of our publishing XML's
 ex:
 ``dotnet publish -c Linux -p:PublishProfile=Linux64`` to to build it standalone.
 
 
 # Credits
 
Li (They/Them)

Supertiger (He/Him)

Olebeck (They/Them)
 
