# PowerShell script to help add CancellationToken support to Azure Functions
# This script identifies functions that need updating

$functionsPath = "D:\Users\Girma.Bewketu\source\repos\Polaris\polaris-gateway\Functions"
$results = @()

Get-ChildItem -Path $functionsPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -ne "BaseFunction.cs" } | ForEach-Object {
	$file = $_
	$content = Get-Content $file.FullName -Raw

	# Check if file contains [Function attribute
	if ($content -match '\[Function\(') {
		$hasSystemThreading = $content -match 'using System\.Threading;'
		$hasCancellationToken = $content -match 'CancellationToken\s+cancellationToken'

		$results += [PSCustomObject]@{
			File = $file.Name
			FullPath = $file.FullName
			HasSystemThreading = $hasSystemThreading
			HasCancellationToken = $hasCancellationToken
			NeedsUpdate = -not ($hasSystemThreading -and $hasCancellationToken)
		}
	}
}

Write-Host "`nSummary of Functions Status:" -ForegroundColor Cyan
Write-Host "============================`n"

$needsUpdate = $results | Where-Object { $_.NeedsUpdate }
$upToDate = $results | Where-Object { -not $_.NeedsUpdate }

Write-Host "Functions needing update: $($needsUpdate.Count)" -ForegroundColor Yellow
Write-Host "Functions up to date: $($upToDate.Count)" -ForegroundColor Green
Write-Host "`nTotal functions: $($results.Count)`n"

if ($needsUpdate.Count -gt 0) {
	Write-Host "`nFunctions that need updating:" -ForegroundColor Yellow
	$needsUpdate | ForEach-Object {
		$status = @()
		if (-not $_.HasSystemThreading) { $status += "missing using System.Threading" }
		if (-not $_.HasCancellationToken) { $status += "missing CancellationToken parameter" }
		Write-Host "  - $($_.File): $($status -join ', ')"
	}
}

# Export results to CSV for reference
$results | Export-Csv -Path "D:\Users\Girma.Bewketu\source\repos\Polaris\cancellation-token-status.csv" -NoTypeInformation
Write-Host "`nDetailed results exported to: D:\Users\Girma.Bewketu\source\repos\Polaris\cancellation-token-status.csv" -ForegroundColor Cyan
