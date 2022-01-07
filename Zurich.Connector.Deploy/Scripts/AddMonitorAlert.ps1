[cmdletbinding()]
param (
	[Parameter(Mandatory = $true)] [string] $env,
	[Parameter(Mandatory = $true)] [string] $subscription,
	[Parameter(Mandatory = $true)] [string] $resourceGroup,
	[Parameter(Mandatory = $true)] [string] $appServiceName,
	[Parameter(Mandatory = $true)] [string] $alertName,
	[Parameter(Mandatory = $true)] [int] $threshold,
	[Parameter(Mandatory = $true)] [string] $windowSize,
	[Parameter(Mandatory = $true)] [string] $evaluationFrequency,
	[Parameter(Mandatory = $true)] [string] $condition,
	[Parameter(Mandatory = $true)] [string[]] $actionGroups,
	[Parameter(Mandatory = $true)] [string] $description
)
	
# helpful doc about metric names
# https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/metrics-supported
function AddMonitorAlert() {
	param(
		[Parameter(Mandatory = $true)]
		[string]     $env,
 
		[Parameter(Mandatory = $true)]
		[string]     $subscription,

		[Parameter(Mandatory = $true)]
		[string]     $resourceGroup,

		[Parameter(Mandatory = $true)]
		[string]     $appServiceName,

		[Parameter(Mandatory = $true)]
		[string]     $alertName,

		[Parameter(Mandatory = $true)]
		[int]     $threshold,

		[Parameter(Mandatory = $true)]
		[string]     $windowSize,
    
		[Parameter(Mandatory = $true)]
		[string]     $evaluationFrequency,
    
		[Parameter(Mandatory = $true)]
		[string]     $condition,
    
		[Parameter(Mandatory = $true)]
		[string[]]     $actionGroups,
    
		[Parameter(Mandatory = $false)]
		[string]     $description = ""
	)

	$allSubscriptions = @(az account list --all --output json) | ConvertFrom-Json
	$sub = $allSubscriptions | Where-Object { $_.name -eq $subscription }
	$subscriptionId = $sub.Id
	$fullResourceName = "$appServiceName$env"
	$resourceFQ = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$fullResourceName"
	$fullAlertName = "$fullResourceName-$alertName"

	Write-Host "Getting list of monitor alerts in $resourceGroup"
	$alerts = @(az monitor metrics alert list --resource-group $resourceGroup --output json) | ConvertFrom-Json
	$currentAlert = $alerts | Where-Object { ($_.name -eq $fullAlertName) }
	if ($currentAlert -and ($currentAlert.criteria.allOf[0].threshold -eq $threshold)) {
		Write-Host "Monitor alert $fullAlertName is already created"
	}
	elseif ($currentAlert) {
		Write-Host "Updating condition threshold for monitor alert $fullAlertName to $threshold"
		az monitor metrics alert update -n $fullAlertName -g $resourceGroup --subscription $subscription --scopes $resourceFQ --set criteria.allOf[0].threshold=$threshold
		continue;
	}
	else {
		Write-Host "Creating monitor alert for $fullResourceName $fullAlertName in $resourceGroup"         
		if (az monitor metrics alert create -n $fullAlertName -g $resourceGroup --subscription $subscription --scopes $resourceFQ --condition $condition --action $actionGroups[0] --description $description --evaluation-frequency $evaluationFrequency --window-size $windowSize) {
			Write-Host "Successfully created monitor alert for $fullAlertName in $resourceGroup`r`n"
			for ($i = 1; $i -lt $actionGroups.Count; $i += 1) {
				Write-Host "Adding action group $actionGroups[$i] to $fullAlertName in $resourceGroup"
				if (az monitor metrics alert update -n $fullAlertName -g $resourceGroup --subscription $subscription --scopes $resourceFQ --add-action $actionGroups[$i]) {
					Write-Host "Successfully added action group $actionGroups[$i] to alert for $fullAlertName in $resourceGroup`r`n"
				}
				else {
					Write-Error "Error occurred adding action group to alert. See above error"
				}
			}
		}
		else {
			Write-Error "Error occurred creating/updating monitor metric alert. See above error"
			return 1
		}
	}
}

AddMonitorAlert -env $env -subscription $subscription -resourceGroup $resourceGroup -appServiceName $appServiceName -alertName $alertName -threshold $threshold -windowSize $windowSize -evaluationfrequency $evaluationfrequency -condition $condition -actionGroups $actionGroups -description $description