Param(
    [String]$InstrumentationKey,
    [String]$PatToken,
    [String]$PipelineName,
    [String]$CommitId,
    [String]$BuildId,
    [String]$BuildName,
    [bool]$Success = $False)

$url="https://dev.azure.com/CPSDTS/Information%20Management/_apis/build/builds/$BuildId/timeline?api-version=6.0"
$token = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes(":$($PatToken)"))
$response = Invoke-RestMethod -Uri $url -Headers @{Authorization = "Basic $token"} -Method Get -ContentType application/json
$errors = $response.records.Where({ $_.result -eq "failed“ })

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

$errors.ForEach({
    $_.issues.ForEach({
        Write-Host "Recording Error"
        $telemetryClient.TrackEvent("Deployment Failed: $($_.message)");
        $telemetryClient.Flush(); 
    })
})