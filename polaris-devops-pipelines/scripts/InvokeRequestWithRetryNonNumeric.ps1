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

$AdjustedSuccessTextContent = $SuccessTextContent.replace('-ci', '').replace('-man', '')

Write-Output "$Method ""$URI"" Retries: $Retries, SecondsDelay $SecondsDelay, TimeoutSec $TimeoutSec, ExpectedVersion $AdjustedSuccessTextContent";

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
                if($response.Content -like "*$AdjustedSuccessTextContent*")
                {
                    Write-Host "Health check validation success - '$AdjustedSuccessTextContent' found."
                    $completed = $true
                }
                else
                {
                    Write-Warning "Invalid version found - expecting content to contain '$AdjustedSuccessTextContent', received '$response.Content'"
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
    $res = Req -Retries $Retries -SecondsDelay $SecondsDelay -Params @{ 'Method' = $Method; 'Uri' = $URI; 'TimeoutSec' = $TimeoutSec; 'UseBasicParsing' = $true }
    if($res -eq $true)
    {
        Write-Host "Health check validation success - '$AdjustedSuccessTextContent' found."
    }
    else
    {
        Write-Error "Health check validation failed - '$AdjustedSuccessTextContent' was never found."
    }
}
catch
{
    Write-Host $PSItem.Exception.Message -ForegroundColor RED
}