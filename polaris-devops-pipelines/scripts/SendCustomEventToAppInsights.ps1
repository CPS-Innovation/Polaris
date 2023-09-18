
Param(
    [String]$InstrumentationKey,
    [String]$Name,
    [String]$Message,
    [bool]$Success = $True)

#insall NuGet package manager if not found
$_nugetUrl = "https://api.nuget.org/v3/index.json"
$packageSources = Get-PackageSource
if(@($packageSources).Where{$_.location -eq $_nugetUrl}.count -eq 0)
{
    Register-PackageSource -Trusted -Name MyNuGet -Location $_nugetUrl -ProviderName NuGet
}

#install Microsoft.ApplicationInsights package
Install-Package -Force Microsoft.ApplicationInsights -RequiredVersion "2.21.0" -Scope CurrentUser

# setup telemetry client ("*ApplicationInsights.dll" DLL must be present)
$telemetryClient  = New-Object -TypeName "Microsoft.ApplicationInsights.TelemetryClient"
$telemetryClient.InstrumentationKey = $InstrumentationKey

try
{
    # initialize context information
    $telemetryClient.Context.Session.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
    $telemetryClient.Context.User.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
    $telemetryClient.Context.Operation.Id = $Name
    $telemetryClient.Context.Operation.Name = $Name

    # setup request telemetry information
    #$request = New-Object -TypeName "Microsoft.ApplicationInsights.DataContracts.EventTelemetry"
    #$request.Name       = $Name

    $p = [pscustomobject]@{
        Str = "Test Message"
    } | ConvertTo-Json

    $measurement = [pscustomobject]@{
        totalTime = 0.33
    } | ConvertTo-Json
    $telemetryClient.TrackEvent($Name, $p, $measurement);

    # track the request
    #$telemetryClient.TrackEvent($request)
}
catch
{
    $telemtryException = New-Object -TypeName "Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry"
    $telemtryException.Exception = $_.Exception
    $client.TrackException($telemtryException)
}
finally
{
    $telemetryClient.Flush()
}
