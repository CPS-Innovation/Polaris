param(
    [String]$ResourceGroupName,
    [String]$AppName
)

Function AssessExlusions {
    param(
        [String]$AppSetting
    )
    
    $updateAllowed = $true
    
    #Azure Settings
    if (($AppSetting.Contains('WEBSITE')) -or ($AppSetting.Contains('APPLICATIONINSIGHTS')) -or ($AppSetting.Contains('APPINSIGHTS')) -or ($AppSetting.Contains('FUNCTIONS'))){
        $updateAllowed = $false
    }
    
    #Site Config
    if (($AppSetting -eq 'HostType') -or ($AppSetting -eq 'CoordinatorTaskHub') -or ($AppSetting -eq 'AzureWebJobsStorage') -or ($AppSetting -eq 'StorageConnectionAppSetting')){
        $updateAllowed = $false
    }
    
    #Function-Specific Config
    if (($AppSetting.Contains('AzureWebJobs'))){
        $updateAllowed = $false
    }
        
    return $updateAllowed
}

$prod = Get-AzWebApp -ResourceGroupName $ResourceGroupName -Name $AppName
$prodAppSettings = $prod.SiteConfig.AppSettings

$slot = Get-AzWebAppSlot -ResourceGroupName $ResourceGroupName -Name $AppName -Slot 'Staging1'
$slotAppSettings = $slot.SiteConfig.AppSettings

$toBeAdded = New-Object System.Collections.ArrayList
$toBeUpdated = New-Object System.Collections.ArrayList
$toBeDeleted = New-Object System.Collections.ArrayList

try
{
    # Derive New/Updated AppSettings
    foreach($prodSetting in $prodAppSettings){
        $result = AssessExlusions $prodSetting.Name
        if ($result -eq $true)
        {
            $updateFlag = $false
            
            foreach ($slotSetting in $slotAppSettings)
            {
                if ($slotSetting.Name -eq $prodSetting.Name)
                {
                    $updateFlag = $true
                    [void]$toBeUpdated.Add([pscustomobject]@{ Name = $prodSetting.Name; Value = $prodSetting.Value; })
                    break
                }
            }
            
            if ($updateFlag -eq $false){
                [void]$toBeAdded.Add([pscustomobject]@{ Name = $prodSetting.Name; Value = $prodSetting.Value; })
            }
        }
    }
    
    # Derive AppSettings to be Deleted
    foreach($slotSetting in $slotAppSettings){
        $result = AssessExlusions $slotSetting.Name
        if ($result -eq $true)
        {
            $deleteFlag = $true
            
            foreach ($prodSetting in $prodAppSettings)
            {
                if ($prodSetting.Name -eq $slotSetting.Name)
                {
                    $deleteFlag = $false
                    break
                }
            }

            if ($deleteFlag -eq $true){
                [void]$toBeDeleted.Add([pscustomobject]@{ Name = $slotSetting.Name; Value = $slotSetting.Value; })
            }
        }
    }

    #now build new appsettings collection
    $newSlotSettings = @{}
    foreach ($slotSetting in $slotAppSettings)
    {
        $result = AssessExlusions $slotSetting.Name
        if ($result -eq $true)
        {
            if (($toBeDeleted.count -gt 0) -and ($toBeDeleted.Name -contains $slotSetting.Name))
            {
                #do nothing, exclude from newSlotSettings collection
                Write-Host "Deleting " $slotSetting.Name -ForegroundColor RED
            }
            elseif (($toBeUpdated.count -gt 0) -and ($toBeUpdated.Name -contains $slotSetting.Name))
            {
                $updateDetails = $toBeUpdated | Where-Object { $_.Name -eq $slotSetting.Name }
                $newSlotSettings[$slotSetting.Name] = $updateDetails.Value
                Write-Host "Updating " $slotSetting.Name -ForegroundColor YELLOW
            }
        }
        else
        {
            $newSlotSettings[$slotSetting.Name] = $slotSetting.Value
            Write-Host "Adding " $slotSetting.Name -ForegroundColor BLUE
        }
    }
    
    if ($toBeAdded.count -gt 0)
    {
        foreach ($addObj in $toBeAdded){
            $newSlotSettings[$addObj.Name] = $addObj.Value
            Write-Host "Adding " $addObj.Name -ForegroundColor BLUE
        }
    }
    
    if ($newSlotSettings.count -gt 0)
    {
        Write-Host "Slot AppSettings Update Started" -ForegroundColor GREEN
        Set-AzWebAppSlot -ResourceGroupName $ResourceGroupName -Name $AppName  -AppSettings $newSlotSettings -Slot "Staging1"
        Write-Host "Slot AppSettings Update Completed" -ForegroundColor GREEN
    }
    
    exit 0
}
catch
{
    Write-Host $PSItem.Exception.Message -ForegroundColor RED
    exit 1
}