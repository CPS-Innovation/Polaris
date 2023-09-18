
Param(
    [String]$InstrumentationKey,
    [String]$Name,
    [String]$Message,
    [bool]$Success = $True)

$appInsightsIngestionEndpoint = "https://uksouth-1.in.applicationinsights.azure.com/";
$customPropertiesObj = [PSCustomObject]@{};
$bodyObject = [PSCustomObject]@{
    'name' = "Microsoft.ApplicationInsights.$InstrumentationKey.Event"
    'time' = ([System.dateTime]::UtcNow.ToString('o'))
    'iKey' = $InstrumentationKey
    'tags' = [PSCustomObject]@{
        'ai.cloud.roleInstance' = $($env:SYSTEM_TEAMPROJECTID)/$Name
        'ai.internal.sdkVersion' = 'AzurePowerShellUtilityFunctions'
    }
    'data' = [PSCustomObject]@{
        'baseType' = 'EventData'
        'baseData' = [PSCustomObject]@{
            'ver' = '2'
            'name' = $Message
            'properties' = $customPropertiesObj
        }
    }
};

# convert the body object into a json blob.
$bodyAsCompressedJson = $bodyObject | ConvertTo-JSON -Depth 10 -Compress;

# prepare the headers
$headers = @{
    'Content-Type' = 'application/x-json-stream';
};


# initialize context information
Write-Host "Initializing Telemetry Client with Context"
$telemetryClient.Context.Session.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
$telemetryClient.Context.User.Id = "$($env:SYSTEM_TEAMPROJECTID)/$($env:BUILD_BUILDID)"
$telemetryClient.Context.Operation.Id = $Name
$telemetryClient.Context.Operation.Name = $Name

try
{
    Write-Host "Calling 'TrackEvent' endpoint with '$Message'"
    Invoke-RestMethod -Uri $appInsightsIngestionEndpoint -Method Post -Headers $headers -Body $bodyAsCompressedJson;
}
catch
{
    Write-Host $PSItem.Exception.Message -ForegroundColor RED
}
finally
{
    Write-Host "Finished writing message to App Insights"
}
