
Param(
    [String]$InstrumentationKey,
    [String]$Name,
    [String]$Message,
    [bool]$Success = $True)

#insall NuGet package manager if not found
Write-Host "Checking for existing NuGet package manager"
$_nugetUrl = "https://api.nuget.org/v3/index.json"
$packageSources = Get-PackageSource
if(@($packageSources).Where{$_.location -eq $_nugetUrl}.count -eq 0)
{
    Write-Host "Installing NuGet package manager"
    Register-PackageSource -Trusted -Name MyNuGet -Location $_nugetUrl -ProviderName NuGet
}

#install Microsoft.ApplicationInsights package
Write-Host "Installing Microsoft.ApplicationInsights"
Install-Package -Force Microsoft.ApplicationInsights -RequiredVersion "2.21.0" -Scope CurrentUser

Write-Host "Instantiating Telemetry Client"
# setup telemetry client ("*ApplicationInsights.dll" DLL must be present)
$telemetryClient  = New-Object -TypeName "Microsoft.ApplicationInsights.TelemetryClient"
$telemetryClient.InstrumentationKey = $InstrumentationKey

# initialize context information
Write-Host "Initializing Telemetry Client with Context"
$telemetryClient.Context.Session.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
$telemetryClient.Context.User.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
$telemetryClient.Context.Operation.Id = $Name
$telemetryClient.Context.Operation.Name = $Name

try
{
    Write-Host "Calling 'TrackEvent' with '$Message'"
    $telemetryClient.TrackEvent($Message);
}
catch
{
    Write-Host "Exception Caught - Recording"
    $telemtryException = New-Object -TypeName "Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry"
    $telemtryException.Exception = $_.Exception
    $telemetryClient.TrackException($telemtryException)
}
finally
{
    Write-Host "Flushing to output stream"
    $telemetryClient.Flush()
    Write-Host "Finished writing message to App Insights"
}
