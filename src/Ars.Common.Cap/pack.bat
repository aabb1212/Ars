dotnet build -c Release
del "bin\Release\*.nupkg" /f /q
dotnet pack -c Release
dotnet nuget push -s https://www.nuget.org/ -k oy2dxokflmzivjckxqma4ahouaza4vzevn3rusdrjwoofy "bin\Release\*.nupkg"
pause