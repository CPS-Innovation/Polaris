Param(
    [String]$InstrumentationKey,
    [String]$PatToken,
    [String]$PipelineName,
    [String]$CommitId,
    [String]$ReleaseId,
    [String]$BuildName,
    [bool]$Success = $False)

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
$customErrors = @{}
@customMetrics = @{}
$errors.ForEach({
    Write-Host "Recording Errors"
    $_.issues.ForEach({
        $customErrors.Add("$($_.name)", "$($_.message)")
    })
})

$telemetryClient.TrackEvent("Deployment Failed", $customErrors, @customMetrics);
$telemetryClient.Flush();