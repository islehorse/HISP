# HISP - Horse Isle Server Protocal.

[![Linux](https://github.com/KuromeSan/HISP/workflows/linux/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Alinux)
[![Windows](https://github.com/KuromeSan/HISP/workflows/windows/badge.svg)](https://github.com/KuromeSan/HISP/actions?query=workflow%3Awindows)

This is an open source re-implementation of the Horse Isle 1 Serverside code
Alot of features don't work atm but it gets in game and let's you do a few things.

# Depends
 HISP Depends on a SQL Server, 
 its been tested and known to work specifically with MariaDB, https://mariadb.org/
 Set its information into server.properties or the server will just crash on first run /-/
 
# Platforms
 The server has been tried and known to work on Windows x86, x64, Linux x64, ARM and ARM64. 
 CI Provides pre-built binaries for all these platforms,
 
# Building
 Building the server from source requires Microsoft .NET Core SDK, targetting version 5.0 https://dotnet.microsoft.com/download/dotnet-core
 
# Web Server
 Theres a bunch of saved files from the Horse Isle 1.0 website, as well as some basic functionality to make it work
 like the original site, the files are in the "WebInterface" folder, it requires PHP 8.0, with the modules "intl" and "mysqli"
