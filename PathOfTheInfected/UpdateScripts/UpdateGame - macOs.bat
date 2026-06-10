
@echo off
REM #push build to itch.io using butler / refinery

REM ### fill out config settings ####

set pathToBuild="C:\PathOfTheInfected\PathOfTheInfected\Build\Macos"
set butlerName=hareltid/strechedup:Macos
@echo on
@echo %pathToBuild%
@echo %butlerName%
@echo off
pause



REM ###run####


cd "C:\Butler\butler"


REM #send file to itch.io via Butler:
butler push %pathToBuild%\ %butlerName%


pause
