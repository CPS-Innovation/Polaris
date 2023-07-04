$personalAccessToken = "PAT TOKEN IN HERE"
$token = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes(":$($personalAccessToken)"))
$header = @{authorization = "Basic $token"}

$organization = "ORG NAME"
$project = "PROJECT NAME"

$pipelineName = "PIPELINE NAME (TITLE)"

#Get all build definitions
$url = "https://dev.azure.com/$organization/$project/_apis/build/definitions?api-version=6.0"
$allBuildDefinitions = Invoke-RestMethod -Uri $url -Method Get -ContentType "application/json" -Headers $header

$allBuildDefinitions.value | Where-Object {$_.name -eq $pipelineName} | ForEach-Object {
    Write-Host $_.id $_.name $_.queueStatus
 
    # For debugging reasons, just to be sure that we don't delete the wrong build pipeline
    if ( $_.name -ne $pipelineName ) {
        return;
    }

    #Get all Builds for a Definition
    $url = "https://dev.azure.com/$organization/$project/_apis/build/builds?definitions=" + $_.id + "&api-version=6.0"
    $allBuildsOfDefinition = Invoke-RestMethod -Uri $url -Method Get -ContentType "application/json" -Headers $header
 
    #Process each Build of Definition
    $allBuildsOfDefinition.value | Where-Object {$_.retainedByRelease -eq "True"} | Sort-Object id | ForEach-Object {
        #Report on retain status
        Write-Host "Build Id:" $_.id " retainedByRelease:" $_.retainedByRelease

        #Get all Retention Leases for this Build
        $url = "https://dev.azure.com/$organization/$project/_apis/build/builds/" + $_.id + "/leases?api-version=7.1-preview.1"
        $allLeasesOfBuild = Invoke-RestMethod -Uri $url -Method Get -ContentType "application/json" -Headers $header

        #Delete each Lease of Build
        $allLeasesOfBuild.value | ForEach-Object {
            #Delete Lease
            $url = "https://dev.azure.com/$organization/$project/_apis/build/retention/leases?ids=" + $_.leaseId + "&api-version=7.1-preview.2"
            Invoke-RestMethod -Uri $url -Method Delete -ContentType "application/json" -Headers $header

            #Report on Lease deleted
            Write-Host "Lease Id:" $_.leaseId " deleted"
        }

        #Delete Build
        $url = "https://dev.azure.com/$organization/$project/_apis/build/builds/" + $_.id + "?api-version=7.1-preview.7"
        Invoke-RestMethod -Uri $url -Method Delete -ContentType "application/json" -Headers $header

        #Report on Build deleted
        Write-Host "Build Id:" $_.id " deleted"
    }

    #Delete the Build Definition
    $url = "https://dev.azure.com/$organization/$project/_apis/build/definitions/" + $_.id + "?api-version=6.0"
    Invoke-RestMethod -Uri $url -Method Delete -ContentType "application/json" -Headers $header
    
    Write-Host "Build Definition:" $pipelineName " (" $_.id ") deleted"
}
    
Write-Host "Done!"