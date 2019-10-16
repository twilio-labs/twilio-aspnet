function updateStandardCsproj() {
  Param($inputFileNameRelative, $targetVersion)

  $inputFileName = Join-Path $PSScriptRoot $inputFileNameRelative
  $fileContent = [xml](Get-Content $inputFileName)
  if ($fileContent.Project.PropertyGroup.Version) {
    $fileContent.Project.PropertyGroup.Version = $targetVersion.ToString()
  }
  $fileContent.Project.PropertyGroup.PackageVersion = $targetVersion.ToString()

  if ($fileContent.Project.ItemGroup) {
    $fileContent.Project.ItemGroup.PackageReference | Where-Object { $_.Include.StartsWith("Twilio") } | ForEach-Object {
      $_.Version = $targetVersion.ToString()
    }
  }

  $fileContent.Save($inputFileName)
}

function updateFrameworkCsproj() {
  Param($inputFileNameRelative, $targetVersion)

  $inputFileName = Join-Path $PSScriptRoot $inputFileNameRelative
  $fileContent = Get-Content -Path $inputFileName -Raw
  $targetVersionString = ("Twilio, Version=" + $targetVersion.ToString() + ".")
  $fileContent = ($fileContent -creplace 'Twilio, Version=\d+\.\d+\.\d+\.', $targetVersionString)
  $targetVersionString = ("\Twilio." + $targetVersion.ToString() + "\")
  $fileContent = ($fileContent -creplace '\\Twilio\.\d+\.\d+\.\d+\\', $targetVersionString)
  $targetVersionString = ("Twilio.AspNet.Common, Version=" + $targetVersion.ToString() + ".")
  $fileContent = ($fileContent -creplace 'Twilio\.AspNet\.Common, Version=\d+\.\d+\.\d+\.', $targetVersionString)
  $targetVersionString = ("\Twilio.AspNet.Common." + $targetVersion.ToString() + "\")
  $fileContent = ($fileContent -creplace '\\Twilio\.AspNet\.Common\.\d+\.\d+\.\d+\\', $targetVersionString)
  $fileContent | Set-Content $inputFileName
}

function updatePackagesConfig() {
  Param($inputFileNameRelative, $targetVersion)

  $inputFileName = Join-Path $PSScriptRoot $inputFileNameRelative
  $fileContent = [xml](Get-Content $inputFileName)
  $fileContent.packages.package | Where-Object { $_.id.StartsWith("Twilio") } | ForEach-Object {
    $_.version = $targetVersion.ToString()
  }

  $fileContent.Save($inputFileName)
}

function updateAssemblyInfo() {
  Param($inputFileNameRelative, $targetVersion)

  $inputFileName = Join-Path $PSScriptRoot $inputFileNameRelative
  $fileContent = Get-Content -Path $inputFileName -Raw
  $targetVersionString = ("AssemblyVersion(`"" + $targetVersion.ToString() + "`")")
  $fileContent = ($fileContent -creplace 'AssemblyVersion\("\d+\.\d+\.\d+"\)', $targetVersionString)
  $targetVersionString = ("AssemblyFileVersion(`"" + $targetVersion.ToString() + "`")")
  $fileContent = ($fileContent -creplace 'AssemblyFileVersion\("\d+\.\d+\.\d+"\)', $targetVersionString)
  $fileContent | Set-Content $inputFileName
}

$currentVersion = [version]([xml](Get-Content src/Twilio.AspNet.Common/Twilio.AspNet.Common.csproj)).Project.PropertyGroup.PackageVersion

$twilioPackageInfo = (ConvertFrom-Json (Invoke-WebRequest https://api.nuget.org/v3/registration3-gz-semver2/twilio/index.json).Content)
$maxVersion = [version]"0.0.0"

$twilioPackageInfo.items | ForEach-Object {
  if ([version]$_.upper -gt $maxVersion) {
    $maxVersion = [version]$_.upper
  }
}

Write-Host "Current Version: $currentVersion"
Write-Host "Latest Twilio package version: $maxVersion"

if ($currentVersion -ge $maxVersion) {
  Throw "Current version is >= Twilio package version. No automation for this case."
}

Write-Host "Bump version to match Twilio package?" -ForegroundColor Yellow
$userInput = Read-Host " ( y / N ) "

if (-not ($userInput -ieq 'y')) {
  Throw "Aborted."
}

Write-Host "Let's do this!"

updateStandardCsproj "src/Twilio.AspNet.Common/Twilio.AspNet.Common.csproj" $maxVersion
updateStandardCsproj "src/Twilio.AspNet.Core/Twilio.AspNet.Core.csproj" $maxVersion
updateFrameworkCsproj "src/Twilio.AspNet.Mvc.UnitTests/Twilio.AspNet.Mvc.UnitTests.csproj" $maxVersion
updateFrameworkCsproj "src/Twilio.AspNet.Mvc/Twilio.AspNet.Mvc.csproj" $maxVersion
updatePackagesConfig "src/Twilio.AspNet.Mvc.UnitTests/packages.config" $maxVersion
updatePackagesConfig "src/Twilio.AspNet.Mvc/packages.config" $maxVersion
updateAssemblyInfo "src/Twilio.AspNet.Mvc.UnitTests/Properties/AssemblyInfo.cs" $maxVersion
updateAssemblyInfo "src/Twilio.AspNet.Mvc/Properties/AssemblyInfo.cs" $maxVersion
