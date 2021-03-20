# HISP - Horse Isle Server Program

[![Linux](https://github.com/KuromeSan/HISP/workflows/linux/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Alinux)
[![Windows](https://github.com/KuromeSan/HISP/workflows/windows/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Awindows)

HISP is a "Server Emulator" for Horse Isle 1          
You may know of "private servers" for big MMO games such as **Runescape** or **Club Penguin**          
well they essentailly run off "Server Emulators".          

btw, alot of features dont work atm, but alot also do, 
i basically have to rewrite the entire game so .. 


**tl;dr, think "Club Penguin Rewritten" but with Horse Isle.**

# Setup
- Download/Clone this repository.          
- Download the latest Windows or Linux binary.        
- Setup a SQL Server, (eg, MariaDB).         
- Setup a webserver with PHP8.0, intl and mysqli modules (eg, Apache).         
- Copy files from this repository's WebInterface/master-site into your webserver.        
  have a separate virtual host for WebInterface/game-site.     
- Edit game-site/config.php to have your SQL login information.         
  Change the server host to your public IP or a Domain that points.       
  to it and also change the admin password        
- Edit master-site/dbconfig.php and set your SQL Server info there as well.       
- Run the HorseIsleServer binary. It will crash due to trying to connect.     
  But will create a "server.properties" file, as well as a CrossDomainPolicy.xml.     
  Edit server.properies and change the DB connection to your SQL server information-.     
  Change whatever other settings you want there as well.       
- Run HorseIsleServer again and this time it will connect to the server and start up.      
- Create an account on the master-site/ webserver.       
  And login using game-site/Horseisle.php. you can also.    
  Give your user account admin using game-site/admin with the password.    
  You set in config.php.     
- That's it your now running HISP Server.       
  Forward the port you used for the server and 80 for the webserver.        
  And people can login over the internet.         


# Depends
 HISP Depends on a SQL Server, 
 its been tested and known to work specifically with MariaDB, https://mariadb.org/
 Set its information into server.properties or the server will just crash on first run /-/
 
# Platforms
 The server has been tried and known to work on Windows x86, x64, Linux x64, ARM and ARM64. 
 CI Provides pre-built binaries for all these platforms,
 
# Building
 Building the server from source requires Microsoft .NET Core SDK, targetting version 5.0 https://dotnet.microsoft.com/download/dotnet-core
 use ``dotnet build`` to build a debug build, (requires .NET Core Runtime) quickly or one of our publishing XML's
 ex:        
 ``dotnet publish -p:PublishProfile=Linux64.pubxml`` to to build it standalone.
 
# Web Server
 Theres a bunch of saved files from the Horse Isle 1.0 website, as well as some basic functionality to make it work
 like the original site, the files are in the "WebInterface" folder, it requires PHP 8.0, with the modules "intl" and "mysqli"
