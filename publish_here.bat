@echo off
setlocal

set OUT=publish

echo Publishing PdfUnlocker to .\%OUT% ...
dotnet publish PdfUnlocker.csproj -c Release -o "%OUT%" --self-contained true -r win-x64
if errorlevel 1 exit /b 1

copy /Y passwords.example.txt "%OUT%\passwords.example.txt" >nul

if not exist "%OUT%\passwords.txt" (
  echo.
  echo Created %OUT%\passwords.example.txt
  echo Copy it to %OUT%\passwords.txt and add YOUR CPF/CNPJ/CEP digit patterns:
  echo   copy "%OUT%\passwords.example.txt" "%OUT%\passwords.txt"
)

echo.
echo Done. Run: %OUT%\PdfUnlocker.exe --help
echo For Send To: shell:sendto then shortcut to %CD%\%OUT%\PdfUnlocker.exe
endlocal
