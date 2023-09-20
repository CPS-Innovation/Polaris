
Param(
    [String]$InstrumentationKey,
    [String]$PipelineName,
    [String]$CommitId,
    [String]$BuildName,
    [String]$Message)

Write-Host "Instantiating Telemetry Client"
# setup telemetry client ("*ApplicationInsights.dll" DLL must be present)
$telemetryClient  = New-Object -TypeName "Microsoft.ApplicationInsights.TelemetryClient"
$telemetryClient.InstrumentationKey = $InstrumentationKey

# initialize context information
Write-Host "Initializing Telemetry Client with Context"
$telemetryClient.Context.Operation.ParentId = $PipelineName
$telemetryClient.Context.Operation.Id = $CommitId
$telemetryClient.Context.Operation.Name = $BuildName
$telemetryClient.Context.Operation.SyntheticSource = "DevOps Release Pipeline"

Write-Host "Calling 'TrackEvent' with '$Message'"
$telemetryClient.TrackEvent($Message);

Write-Host "Flushing to output stream"
$telemetryClient.Flush()
Write-Host "Finished writing message to App Insights"