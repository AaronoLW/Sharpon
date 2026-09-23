@echo off
mkdir build\Windows
mkdir build\Windows\Assets

dotnet publish
copy bin\Release\net10.0\win-x64\publish\Sharpon.exe build\Windows
copy bin\Release\net10.0\win-x64\publish\SDL3* build\Windows
copy Assets build\Windows\Assets
@REM fuck microslop