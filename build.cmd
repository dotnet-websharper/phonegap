@echo off
powershell tools\clean.ps1
powershell tools\configure.ps1
powershell tools\build.ps1
powershell tools\pack.ps1
