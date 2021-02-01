# HISP - Horse Isle Server Protocal.

[![Linux](https://github.com/KuromeSan/HISP/workflows/linux/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Alinux)
[![Windows](https://github.com/KuromeSan/HISP/workflows/windows/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Awindows)

HISP is a "Server Emulator" for Horse Isle 1.0
You may know of "private servers" for big MMO games such as **Runescape** or **Club Penguin**
well they essentailly run off "Server Emulators". 

btw, alot of features dont work atm, but alot also do, 
i basically have to rewrite the entire game so .. 

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
