
Param(
    [String]$InstrumentationKey,
    [String]$Name,
    [String]$Message,
    [bool]$Success = $True)

#install Microsoft.ApplicationInsights package
Install-Package Microsoft.ApplicationInsights -RequiredVersion "2.21.0"

# setup telemetry client ("*ApplicationInsights.dll" DLL must be present)
$telemetryClient  = New-Object -TypeName "Microsoft.ApplicationInsights.TelemetryClient"
$telemetryClient.InstrumentationKey     = $InstrumentationKey

try
{
    # initialize context information
    $telemetryClient.Context.Session.Id     = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
    $telemetryClient.Context.User.Id        = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
    $telemetryClient.Context.Operation.Id   = $Name
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
