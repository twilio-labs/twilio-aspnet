Push-Location .\src

msbuild /t:clean /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }

msbuild /p:Configuration=Release /verbosity:minimal
if ($lastExitCode -ne 0) { exit $lastExitCode }

# Create the NuGet Package

nuget pack .\Twilio.AspNet.Mvc\Twilio.AspNet.Mvc.csproj -Properties configuration=Release -OutputDirectory ..\ -Verbosity quiet
if ($lastExitCode -ne 0) { exit $lastExitCode }

Pop-Location
