
Param(
    [String]$InstrumentationKey,
    [String]$Name,
    [String]$Source,
    [String]$Message,
    [bool]$Success = $True)

$customProperties = @{ ProjectID = $($env:SYSTEM_TEAMPROJECTID); BuildId = $($env:BUILD_BUILDID); SourceVersion = $($env:BUILD_SOURCEVERSION); Name = $Name; Source = $Source; Success = $Success }
$customPropertiesObj = [PSCustomObject]$customProperties;
$appInsightsIngestionEndpoint = "https://uksouth-1.in.applicationinsights.azure.com/";
$bodyObject = [PSCustomObject]@{
    'name' = "Microsoft.ApplicationInsights.$InstrumentationKey.Event"
    'time' = ([System.dateTime]::UtcNow.ToString('o'))
    'iKey' = $InstrumentationKey
    'tags' = [PSCustomObject]@{
        'ai.cloud.roleInstance' = $ENV:COMPUTERNAME
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
