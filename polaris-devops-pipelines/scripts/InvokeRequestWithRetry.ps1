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
$AdjustedSuccessTextContentWithoutVersion = $AdjustedSuccessTextContent.Substring(0, $AdjustedSuccessTextContent.LastIndexOf('.'))
$PotentialVersionNumberText = $AdjustedSuccessTextContent.Substring($AdjustedSuccessTextContent.LastIndexOf('.')+1, $AdjustedSuccessTextContent.Length - $AdjustedSuccessTextContent.LastIndexOf('.')-1)
$PotentialVersionNumber1 = [int32]$PotentialVersionNumberText + 1
$PotentialVersionNumber2 = [int32]$PotentialVersionNumberText + 2
$PotentialVersionNumber3 = [int32]$PotentialVersionNumberText + 3
$PotentialVersionNumber4 = [int32]$PotentialVersionNumberText + 4
$PotentialVersionNumber5 = [int32]$PotentialVersionNumberText + 5
$PotentialSuccessTextContent1 = $AdjustedSuccessTextContentWithoutVersion + '.' + $PotentialVersionNumber1
$PotentialSuccessTextContent2 = $AdjustedSuccessTextContentWithoutVersion + '.' + $PotentialVersionNumber2
$PotentialSuccessTextContent3 = $AdjustedSuccessTextContentWithoutVersion + '.' + $PotentialVersionNumber3
$PotentialSuccessTextContent4 = $AdjustedSuccessTextContentWithoutVersion + '.' + $PotentialVersionNumber4
$PotentialSuccessTextContent5 = $AdjustedSuccessTextContentWithoutVersion + '.' + $PotentialVersionNumber5

Write-Output "$Method ""$URI"" Retries: $Retries, SecondsDelay $SecondsDelay, TimeoutSec $TimeoutSec, ExpectedVersionMin $AdjustedSuccessTextContent, ExpectedVersionMax $PotentialSuccessTextContent5";

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
                if($response.Content -like "*$AdjustedSuccessTextContent*" -Or $response.Content -like "*$PotentialSuccessTextContent1*" -Or $response.Content -like "*$PotentialSuccessTextContent2*" -Or $response.Content -like "*$PotentialSuccessTextContent3*" -Or $response.Content -like "*$PotentialSuccessTextContent4*" -Or $response.Content -like "*$PotentialSuccessTextContent5*")
                {
                    Write-Host "Health check validation success - version range between '$AdjustedSuccessTextContent' and '$PotentialSuccessTextContent5' found."
                    $completed = $true
                }
                else
                {
                    Write-Warning "Invalid version found - expecting content to a version ranged between '$AdjustedSuccessTextContent' and '$PotentialSuccessTextContent5'; received '$response.Content' instead."
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
        Write-Host "Health check validation success - a version between a range of '$AdjustedSuccessTextContent' or '$PotentialSuccessTextContent5' was found."
    }
    else
    {
        Write-Error "Health check validation failed - a version falling between the range of '$AdjustedSuccessTextContent' or '$PotentialSuccessTextContent5' was never received."
    }
}
catch 
{
    Write-Host $PSItem.Exception.Message -ForegroundColor RED    
}