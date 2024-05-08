## Base code borrowed from https://karask.com/retry-powershell-invoke-webrequest/
## Slightly modified to remove things like the file logging.

param (
    [string]$URI,
    [string]$Method = 'GET',
    [string]$SuccessTextContent = 'Healthy',
    [string]$Retries = 1,
    [string]$SecondsDelay = 2,
    [string]$TimeoutSec = 120
)

Function Req {
    Param(
        [Parameter(Mandatory=$True)]
        [hashtable]$Params,
        [int]$Retries = 1,
        [int]$SecondsDelay = 2
    )

    $Params.Add('UserAgent', 'azagent powershell task')

    $method = $Params['Method']
    $url = $Params['Uri']

    $cmd = { Write-Host "$method $url..." -NoNewline; Invoke-WebRequest @Params }

    $retryCount = 0
    $completed = $false
    $response = $null

    while (-not $completed) {
        try {
            $response = Invoke-Command $cmd -ArgumentList $Params
            Write-Information "Result: '$response.StatusCode'"            
            if ($response.StatusCode -ne 200) 
            {
                Write-Warning "Expecting reponse code 200, code received was: $($response.StatusCode)"
            }
            else 
            {
                $jsonResponse = $response.Content | Out-String | ConvertFrom-Json
                $currentBuildVersion = $jsonResponse.buildVersion.replace('-ci', '').replace('-man', '')
                $currentBuildVersionNumber = [Int64]$currentBuildVersion.replace('.','')
                if($currentBuildVersionNumber -ge $potentialBuildVersionNumber)
                {
                    Write-Host "Health check validation success - version matched '$adjustedSuccessTextContent' or was greater (response was $currentBuildVersion)."
                    $completed = $true
                }
                else
                {
                    Write-Warning "Invalid version found - expecting content to be '$adjustedSuccessTextContent' or greater but received '$currentBuildVersion' instead."
                }
            }
            
            if (-not $completed)
            {
                if ($retrycount -ge $Retries) {
                    throw "Request to $url failed the maximum number of $retryCount times."
                } else {
                    Write-Warning "Retrying in $SecondsDelay seconds."
                    Start-Sleep $SecondsDelay
                    $retrycount++
                }   
            }
        } catch {
            Write-Host "$(Get-Date -Format G): Request to $url failed. $_"
            if ($retrycount -ge $Retries) {
                throw "Request to $url failed the maximum number of $retryCount times."
            } else {
                Write-Warning "Request to $url failed. Retrying in $SecondsDelay seconds."
                Start-Sleep $SecondsDelay
                $retrycount++
            }
        }
    }

    Write-Host "OK ($($response.StatusCode))"
    return $completed
}

try
{
    $adjustedSuccessTextContent = $SuccessTextContent.replace('-ci', '').replace('-man', '')
    $potentialBuildVersionNumber = [int64]$adjustedSuccessTextContent.Replace('.','')

    Write-Output "$Method ""$URI"" Retries: $Retries, SecondsDelay $SecondsDelay, TimeoutSec $TimeoutSec, ExpectedVersionMin $adjustedSuccessTextContent";

    $res = Req -Retries $Retries -SecondsDelay $SecondsDelay -Params @{ 'Method' = $Method; 'Uri' = $URI; 'TimeoutSec' = $TimeoutSec; 'UseBasicParsing' = $true }
    if($res -eq $true)
    {
        Write-Host "Health check validation success - '$adjustedSuccessTextContent' or greater was found."
        exit 0
    }
    else
    {
        Write-Error "Health check validation failed - expected a build version matching or greater than '$adjustedSuccessTextContent', but was never received."
        exit 1
    }
}
catch 
{
    Write-Host $PSItem.Exception.Message -ForegroundColor RED
    exit 1
}