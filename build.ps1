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

Write-Host "`nRemoving packages, bin, and obj folders`n" -ForegroundColor DarkBlue -BackgroundColor White
Remove-EntirePath .\packages
Remove-EntirePath .\Twilio.AspNet.Common\bin
Remove-EntirePath .\Twilio.AspNet.Common\obj
Remove-EntirePath .\Twilio.AspNet.Core\bin
Remove-EntirePath .\Twilio.AspNet.Core\obj
Remove-EntirePath .\Twilio.AspNet.Core.UnitTests\bin
Remove-EntirePath .\Twilio.AspNet.Core.UnitTests\obj
Remove-EntirePath .\Twilio.AspNet.Mvc\bin
Remove-EntirePath .\Twilio.AspNet.Mvc\obj
Remove-EntirePath .\Twilio.AspNet.Mvc.UnitTests\bin
Remove-EntirePath .\Twilio.AspNet.Mvc.UnitTests\obj

# Build Twilio.AspNet.Common first
Write-Host "`nBuilding Twilio.AspNet.Common`n" -ForegroundColor DarkBlue -BackgroundColor White
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

Write-Host "`nRestoring packages for solution`n" -ForegroundColor DarkBlue -BackgroundColor White
& $nugetExe ('restore', '-Source', ($PSScriptRoot + ';https://api.nuget.org/v3/index.json'))
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Write-Host "`nCleaning solution`n" -ForegroundColor DarkBlue -BackgroundColor White
dotnet clean -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Write-Host "`nBuilding solution`n" -ForegroundColor DarkBlue -BackgroundColor White
dotnet build -c Release -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

# Run tests
Write-Host "`nTesting Twilio.AspNet.Mvc.UnitTests`n" -ForegroundColor DarkBlue -BackgroundColor White
& $xunitExe (".\Twilio.AspNet.Mvc.UnitTests\bin\Release\Twilio.AspNet.Mvc.UnitTests.dll")
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Write-Host "`nTesting Twilio.AspNet.Core.UnitTests`n" -ForegroundColor DarkBlue -BackgroundColor White
Push-Location .\Twilio.AspNet.Core.UnitTests
dotnet test -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
Pop-Location

# Create the NuGet Packages
Write-Host "`nCreating Twilio.AspNet.Core package`n" -ForegroundColor DarkBlue -BackgroundColor White
Push-Location .\Twilio.AspNet.Core
dotnet pack -c Release -o ..\..\ -v m
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }
Pop-Location

Write-Host "`nCreating Twilio.AspNet.Mvc package`n" -ForegroundColor DarkBlue -BackgroundColor White
& $nugetExe ('pack', '.\Twilio.AspNet.Mvc\Twilio.AspNet.Mvc.csproj', '-Properties', 'configuration=Release', '-OutputDirectory', '..\', '-Verbosity', 'quiet', '-NoPackageAnalysis')
if ($lastExitCode -ne 0) { Stop-Script $lastExitCode }

Pop-Location

Remove-Item Twilio.AspNet.*.deps.json
Remove-Item Twilio.AspNet.*.dll
Remove-Item Twilio.AspNet.*.pdb

Stop-Script 0
