# HISP - Horse Isle Server Protocol

[![Build](https://github.com/islehorse/HISP/workflows/build/badge.svg)](https://github.com/islehorse/HISP/actions?query=workflow%3Abuild)

HISP is a "Server Emulator" for Horse Isle 1          
You may know of "private servers" for big MMO games such as **Runescape** or **Club Penguin**          
well they essentailly run off "Server Emulators".          

!!! ALL FEATURES NOW IMPLEMENTED (um, unless theres some obscure thing i dont know about :D)

**tl;dr, think "Club Penguin Rewritten" but with Horse Isle.**


## Running a Server

Note: if you just want to play HI1 as a single player thing to mess around with;
it is probably preferable to download the HISP-N00BS version; its basically one-click for this

these loose instructions assume somewhat\* knowing how to get around a linux system;
and also assume your using linux, it is possible to setup on other platforms (such as windows)
however this is not documented at the moment


### Setup with Docker

download: https://github.com/islehorse/HISP/blob/master/docker/docker-compose-production.yml

& name it "docker-compose.yml"

create a file named ".env" and include the following
```
DB_GAME1=game1
DB_WEB=web
DB_USER=horseisle
DB_PASS=test123
```

after that; just run 
```
docker compose up -d
```

should give you the following services: 

master-site: localhost:12323
game-site: localhost:12322

hispd: localhost:12321

setup either nginx, apache, or whatever else to reverse-proxy both 12323 & 12322
then continue to the `Configuring HISP` section.

### Manual Download

For the absolute latest, you can grab the [latest build artifact](https://github.com/islehorse/HISP/actions/workflows/build.yml)
otherwise use the [latest release](https://github.com/islehorse/HISP/releases)

if your on a systemd based system, you can run ``./hispd --install-service` 
to create a service file otherwise. simply run the hispd binary in the background;

by default hispd will load configuration and assets from the working directory;
and to log to stdout;

this can be changed via environment variables:
```
HISP_ASSETS_DIR
HISP_CONFIG_DIR
HISP_LOG_FILE
HISP_CONFIG_FILE
```

or the command line arguments:
```
--config-file
--log-to-file
--config-directory
--assets-directory
```

any value in the server.properties file, can be overridden by creating an environment variable, prefixed with HISP_ and then the configuration name
(eg; `HISP_FIX_OFFICIAL_BUGS=true`)

### Debian APT:

Install on Ubuntu or Debian via APT:
```
sudo curl https://silica.codes/api/packages/islehorse/debian/repository.key -o /etc/apt/keyrings/forgejo-islehorse.asc
echo "deb [signed-by=/etc/apt/keyrings/forgejo-islehorse.asc] https://silica.codes/api/packages/islehorse/debian debian main" | sudo tee -a /etc/apt/sources.list.d/hisp.list
sudo apt update
```

Then simply edit /etc/hisp/server.properties & change to correct database credentials
and start the server using `systemctl start hisp`

continue to the `Configuring HISP` section.

### PHP Websites:

Website is built in PHP 8.0 and based on the original Horse Isle Game Website
requires the following PHP modules to be loaded; "intl", "mysqli" and "mysqlnd" on Ubuntu

it can be configured at ``/etc/hisp/web.cfg`` & ``/etc/hisp/game1.cfg`` respectively;
you may need to ensure both these folders are writable by ``www-data`` user;
or whatever your web server is running on.

### Configuring HISP
configuration can be found at ``/etc/hisp` 

(note: configuration file variables can be overridden via environment variables;
prefixed with HISP_ or WEB_ respectively, if your using the default Docker Compose, 
any changes to the database settings will not apply .)

you may need to change certain things (such as domains or ports) in there


`/etc/hisp/web.cfg` - main website config (eg, hi1.horseisle.com)
`/etc/hisp/servers.json` - server list (after logging in)
`/etc/hisp/game1.cfg` - game website config (eg, pinto.horseisle.com)
`/etc/hisp/server.properties` - Game server config
`/etc/hisp/CrossDomainPolicy.xml` - [Adobe Flash XMLSocket Policy](https://clients.sisrv.net/knowledgebase/80/How-to-setup-Flash-Socket-Policy-File.html)

(note: certain configs might not appear right away, and may require acessing a given service once for them to be generated.)


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
 use `dotnet build` to build a debug build, (requires .NET Core Runtime) quickly or one of our publishing XML's
 ex:
 `dotnet publish -c Linux -p:PublishProfile=Linux64` to to build it standalone.
 
 
 # Credits
 
Li (They/Them)

Supertiger (He/Him)

Olebeck (They/Them)
 
