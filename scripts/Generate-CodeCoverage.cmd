@echo off
REM Generate Code Coverage Report
echo.
echo ╔════════════════════════════════════════════════════╗
echo ║       GENERATING CODE COVERAGE REPORT              ║
echo ╚════════════════════════════════════════════════════╝
echo.

REM Clean up old reports
if exist coverage-report rmdir /s /q coverage-report
if exist TestResults rmdir /s /q TestResults

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo   STEP 1: RUNNING TESTS WITH COVERAGE COLLECTION
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.

echo Running tests with coverage collection...
dotnet test ErpBE.Tests --collect:"XPlat Code Coverage" --logger:"console;verbosity=minimal"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Tests failed. Coverage report may be incomplete.
    exit /b 1
)

echo.
echo ✅ Tests completed successfully
echo.

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo   STEP 2: GENERATING HTML REPORT
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.

REM Find coverage file (recursive search in TestResults)
set COVERAGE_FILE=
for /r TestResults %%f in (*.cobertura.xml) do (
    set COVERAGE_FILE=%%f
    goto :found
)

:found
if not defined COVERAGE_FILE (
    echo ❌ No coverage file found!
    echo Searched in: %CD%\TestResults
    dir /s /b TestResults\*.xml 2>nul
    exit /b 1
)

echo 📊 Coverage file: %COVERAGE_FILE%
echo.

echo Generating HTML report...
reportgenerator "-reports:%COVERAGE_FILE%" "-targetdir:coverage-report" "-reporttypes:Html" "-verbosity:Warning"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Failed to generate report
    exit /b 1
)

echo.
echo ✅ Report generated successfully
echo.

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo   COVERAGE REPORT SUMMARY
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.

if exist coverage-report\Summary.txt (
    findstr /C:"Line coverage" /C:"Branch coverage" coverage-report\Summary.txt
)

echo.
echo 📁 Report Location: %CD%\coverage-report
echo 🌐 HTML Report: file:///%CD%\coverage-report\index.html
echo.

echo ╔════════════════════════════════════════════════════╗
echo ║          CODE COVERAGE REPORT COMPLETE             ║
echo ╚════════════════════════════════════════════════════╝
echo.

REM Open report in browser
start coverage-report\index.html

echo Report opened in browser!
echo.

