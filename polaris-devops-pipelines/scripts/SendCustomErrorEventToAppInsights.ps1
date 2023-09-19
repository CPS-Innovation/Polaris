Param(
    [String]$InstrumentationKey,
    [String]$PatToken,
    [String]$PipelineName,
    [String]$CommitId,
    [String]$ReleaseId,
    [String]$BuildName,
    [String]$Message)

$url="https://dev.azure.com/CPSDTS/Information%20Management/_apis/build/builds/$ReleaseId/timeline?api-version=6.0"
Write-Host "Getting errors from $($url)"
$token = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes(":$($PatToken)"))
$response = Invoke-RestMethod -Uri $url -Headers @{Authorization = "Basic $token"} -Method Get -ContentType application/json
$errors = $response.records.Where({ $_.result -eq "failed“ })
Write-Host "$($errors.Count) errors found"

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

#add all errors to a hashtable
$customMetrics = New-Object "System.Collections.Generic.Dictionary``2[System.String,System.Double]"
$customErrors = New-Object "System.Collections.Generic.Dictionary``2[System.String,System.String]"
$errors.ForEach({
    Write-Host "Recording Errors"
    $incre = 0
    $_.issues.ForEach({
        $incre++
        $customErrors.Add("Error $($incre)", "$($_.message)")
    })
})

Write-Host "Calling 'TrackEvent' with '$Message'"
$telemetryClient.TrackEvent($Message, $customErrors, $customMetrics);

Write-Host "Flushing to output stream"
$telemetryClient.Flush();
Write-Host "Finished writing message to App Insights"