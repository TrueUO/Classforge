@SET CURPATH=%~dp0

@SET EXENAME=Classforge

@TITLE: %EXENAME% - https://github.com/TrueUO/Classforge

::##########

@ECHO:
@ECHO: Compile %EXENAME% for Windows
@ECHO:

@PAUSE

dotnet build -c Release

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS

::##########

@ECHO OFF

"%CURPATH%%EXENAME%.exe"

