function Remove-EntirePath() {
  Param([string]$Path)
  if(Test-Path $Path) {
    Remove-Item -Recurse -Force $Path
  }
}

Push-Location .\src

Remove-EntirePath .\Twilio.AspNet.Common\bin
Remove-EntirePath .\Twilio.AspNet.Common\obj
Remove-EntirePath .\Twilio.AspNet.Core\bin
Remove-EntirePath .\Twilio.AspNet.Core\obj
Remove-EntirePath .\Twilio.AspNet.Mvc\bin
Remove-EntirePath .\Twilio.AspNet.Mvc\obj

nuget restore
if ($lastExitCode -ne 0) { exit $lastExitCode }

msbuild /t:clean /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }

msbuild /p:Configuration=Release /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }

# Create the NuGet Packages

Push-Location .\Twilio.AspNet.Common
msbuild /t:pack /p:Configuration=Release /p:OutputPath=..\..\ /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }
Pop-Location

Push-Location .\Twilio.AspNet.Core
msbuild /t:pack /p:Configuration=Release /p:OutputPath=..\..\ /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }
Pop-Location

nuget pack .\Twilio.AspNet.Mvc\Twilio.AspNet.Mvc.csproj -Properties configuration=Release -OutputDirectory ..\ -Verbosity quiet
if ($lastExitCode -ne 0) { exit $lastExitCode }

Pop-Location

Remove-Item Twilio.AspNet.*.deps.json
Remove-Item Twilio.AspNet.*.dll
Remove-Item Twilio.AspNet.*.pdb
