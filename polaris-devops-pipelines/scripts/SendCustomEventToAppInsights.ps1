
Param(
    [String]$InstrumentationKey,
    [String]$PipelineName,
    [String]$CommitId,
    [String]$BuildId,
    [String]$Message,
    [bool]$Success = $True)

Write-Host "Instantiating Telemetry Client"
# setup telemetry client ("*ApplicationInsights.dll" DLL must be present)
$telemetryClient  = New-Object -TypeName "Microsoft.ApplicationInsights.TelemetryClient"
$telemetryClient.InstrumentationKey = $InstrumentationKey

# initialize context information
Write-Host "Initializing Telemetry Client with Context"
$telemetryClient.Context.Operation.ParentId = $PipelineName
$telemetryClient.Context.Operation.Id = $CommitId
$telemetryClient.Context.Operation.Name = $BuildId
$telemetryClient.Context.Operation.SyntheticSource = "DevOps Release Pipeline"

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
