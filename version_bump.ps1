#!/usr/bin/env pwsh
function updateStandardCsproj() {
  Param($inputFileNameRelative, $targetVersion)

  Write-Host "Updating : $inputFileNameRelative"

  $inputFileName = Join-Path $PSScriptRoot $inputFileNameRelative
  $fileContent = [xml](Get-Content $inputFileName)
  if ($fileContent.Project.PropertyGroup.Version) {
    $fileContent.Project.PropertyGroup.Version = $targetVersion.ToString()
  }
  if ($fileContent.Project.PropertyGroup.PackageVersion) {
    $fileContent.Project.PropertyGroup.PackageVersion = $targetVersion.ToString()
  }

  $fileContent.Project.ItemGroup `
  | Where-Object { $_.PackageReference -ne $Null } `
  | Select-Object -ExpandProperty PackageReference `
  | Where-Object { $_.Include -ne $Null -and $_.Include.StartsWith("Twilio") } `
  | ForEach-Object {
      $_.Version = $targetVersion.ToString()
    }

  $fileContent.Save($inputFileName)
}

$currentVersion = [version]([xml](Get-Content src/Twilio.AspNet.Common/Twilio.AspNet.Common.csproj)).Project.PropertyGroup.PackageVersion

$twilioPackageInfo = (Invoke-WebRequest https://www.nuget.org/packages/Twilio/latest).Content
$maxVersion = [version]"0.0.0"

if ($twilioPackageInfo -match "\| Twilio (\d+\.\d+\.\d+)") {
  $maxVersion = [version]$matches[1]
}

Write-Host "Current Version: $currentVersion"
Write-Host "Latest Twilio package version: $maxVersion"

# if ($currentVersion -ge $maxVersion) {
#   Throw "Current version is >= Twilio package version. No automation for this case."
# }

Write-Host "Bump version to match Twilio package?" -ForegroundColor Yellow
$userInput = Read-Host " ( y / N ) "

if (-not ($userInput -ieq 'y')) {
  Throw "Aborted."
}

Write-Host "Let's do this!"

updateStandardCsproj "src/Twilio.AspNet.Common/Twilio.AspNet.Common.csproj" $maxVersion
updateStandardCsproj "src/Twilio.AspNet.Core/Twilio.AspNet.Core.csproj" $maxVersion
updateStandardCsproj "src/Twilio.AspNet.Core.UnitTests/Twilio.AspNet.Core.UnitTests.csproj" $maxVersion
updateStandardCsproj "src/Twilio.AspNet.Mvc/Twilio.AspNet.Mvc.csproj" $maxVersion
updateStandardCsproj "src/Twilio.AspNet.Mvc.UnitTests/Twilio.AspNet.Mvc.UnitTests.csproj" $maxVersion