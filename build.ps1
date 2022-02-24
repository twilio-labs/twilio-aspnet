$originalLocation = Get-Location

function Remove-EntirePath() {
  Param([string]$Path)
  if (Test-Path $Path) {
    Remove-Item -Recurse -Force $Path
  }
}

function Stop-Script($exitCode) {
  Set-Location $originalLocation
  if ($exitCode -eq 0) {
    Write-Host "SUCCESS" -ForegroundColor Green
  } else {
    Write-Host "FAILURE" -ForegroundColor Red
  }
  exit $exitCode
}

$nugetExe = Join-Path $PSScriptRoot "tools/nuget.exe"
$xunitExe = Join-Path $PSScriptRoot "tools/xunit.console.x86.exe"

Push-Location .\src

Remove-EntirePath .\Twilio.AspNet.Common\bin
Remove-EntirePath .\Twilio.AspNet.Common\obj
Remove-EntirePath .\Twilio.AspNet.Core\bin
Remove-EntirePath .\Twilio.AspNet.Core\obj
Remove-EntirePath .\Twilio.AspNet.Mvc\bin
Remove-EntirePath .\Twilio.AspNet.Mvc\obj

# Build Twilio.AspNet.Common first
Push-Location .\Twilio.AspNet.Common
dotnet restore -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
dotnet clean -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
dotnet build -c Release -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
dotnet pack -c Release -o ..\..\ -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
Pop-Location

dotnet restore -v m --source $($PSScriptRoot + ';https://api.nuget.org/v3/index.json')
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

dotnet clean -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

dotnet build -c Release -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

# Run tests
& $xunitExe (".\Twilio.AspNet.Mvc.UnitTests\bin\Release\Twilio.AspNet.Mvc.UnitTests.dll")
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Push-Location .\Twilio.AspNet.Core.UnitTests
dotnet test -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
Pop-Location

# Create the NuGet Packages
Push-Location .\Twilio.AspNet.Core
dotnet pack -c Release -o ..\..\ -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
Pop-Location

& $nugetExe ('pack', '.\Twilio.AspNet.Mvc\Twilio.AspNet.Mvc.csproj', '-Properties', 'configuration=Release', '-OutputDirectory', '..\', '-Verbosity', 'quiet')
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Pop-Location

Remove-Item Twilio.AspNet.*.deps.json
Remove-Item Twilio.AspNet.*.dll
Remove-Item Twilio.AspNet.*.pdb

Stop-Script 0
