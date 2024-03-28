# Disable status info to clean up build agent stdout
$global:ProgressPreference = 'SilentlyContinue'
$global:VerbosePreference = "SilentlyContinue"

$azureRmModule = Get-InstalledModule AzureRM -ErrorAction SilentlyContinue
if ($azureRmModule) {
    Write-Host 'AzureRM module exists. Removing it'
    Uninstall-Module -Name AzureRM -AllVersions
    Write-Host 'AzureRM module removed'
}

Write-Host 'Installing Az module...'
Install-Module Az -Force -AllowClobber

if (Get-Command Uninstall-AzureRm -ErrorAction SilentlyContinue) {
    Write-Host 'Running Uninstall-AzureRm...'
    Uninstall-AzureRm
}