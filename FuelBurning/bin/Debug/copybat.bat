@echo off
setlocal
set PATH="C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\[A17]-BurnTheFuel\Assemblies\"
cd /d %~dp0
copy /y *.dll %PATH%
endlocal