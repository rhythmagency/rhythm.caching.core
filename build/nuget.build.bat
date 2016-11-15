REM This file should not be run directly.
REM It is run by the Visual Studio buld process.

SET NUGET=..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe
SET VER=%1

@ECHO ====== Creating NuGet Package in "dist" Folder
%NUGET% pack Rhythm.Caching.Core.csproj -outputdirectory ..\..\dist -version %VER%