[cmdletbinding()]
param (
    [Parameter(Mandatory=$true)] [string] $subscription,
    [Parameter(Mandatory=$true)] [string] $subscriptionId,
    [Parameter(Mandatory=$true)] [string] $resourceGroup,
    [Parameter(Mandatory=$true)] [string] $env,
    [Parameter(Mandatory=$true)] [string] $alertPrefix,
    [Parameter(Mandatory=$true)] [string[]] $monitorGroups
)
function Main(){

	$envName = $env

	$actionGroups = [System.Collections.ArrayList]@()
	foreach($group in $monitorGroups){
		$actionGroups.Add("/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Insights/actionGroups/$group")
	}


	#fully qualified resource ID templates
	$fqResourceAppServicePlan = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Web/serverFarms/"
	$fqResourceAppService = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/"

	# TODO: Arguably this shouldn't be hardcoded
	$appServices = @(@{AppService="connectorsWebApi$envName"})
    $appServicePlans = @($appServices | ForEach-Object {@{AppServicePlan="AppServicePlan-$($_.AppService)"}})	

	$alertMap = New-Object 'system.collections.generic.dictionary[string,System.Collections.Hashtable[]]'

	$alertMap.Add("RespTime",$appServices)
	$alertMap.Add("ServerError",$appServices)
	$alertMap.Add("CPU",$appServicePlans)
	$alertMap.Add("RAM",$appServicePlans)
	$alertMap.Add("ClientError",$appServices)	

	Write-Host "Getting list of monitor alerts in $resourceGroup"
	$alerts = @(az monitor metrics alert list --resource-group $resourceGroup --output json) | ConvertFrom-Json

	foreach($alertKey in $alertMap.Keys){
		$alertValues = $alertMap[$alertKey]
		#variables
		$name = ""
		$condition = ""
		$description = ""
	
		foreach($resource in $alertValues){
			$resourceFQ = ""
			$resourceShort = $($resource.Values[0])

			#alert scope
			#Need to update $threshold when updating condition
			if ($envName -eq "CI") {
				switch($alertKey){
					"RespTime" { $name = "HighRespTime"; $condition = "avg averageresponsetime > 10"; $threshold = 10; $description = "High average response time detected on $resourceShort"; break }
					"ServerError" { $name = "ServerErrors"; $condition = "total http5xx > 50"; $threshold = 50; $description = "High server errors detected on $resourceShort"; break }
					"ClientError" { $name = "ClientErrors"; $condition = "total http4xx > 300"; $threshold = 300; $description = "High client errors detected on $resourceShort"; break }
					"CPU" { $name = "HighCPU"; $condition = "avg cpupercentage > 90"; $threshold = 90; $description = "High CPU usage detected on $resourceShort"; break }
					"RAM" { $name = "HighRAMUsage"; $condition = "avg memorypercentage > 85"; $threshold = 85; $description = "High RAM utilization detected on $resourceShort"; break }
					"AGFailures" { $name = "HighFailedRequests"; $condition = "total failedrequests > 50"; $threshold = 50; $description = "High failed requests on $resourceShort"; break }
					"StorageAvailability" {  $name = "Availability"; $condition = "avg availability < 98"; $threshold = 98; $description = "Reduced storage availability on $resourceShort"; break }
				}
			} else {
				switch($alertKey){
					"RespTime" { $name = "HighRespTime"; $condition = "avg averageresponsetime > 10"; $threshold = 10; $description = "High average response time detected on $resourceShort"; break }
					"ServerError" { $name = "ServerErrors"; $condition = "total http5xx > 30"; $threshold = 30; $description = "High server errors detected on $resourceShort"; break }
					"ClientError" { $name = "ClientErrors"; $condition = "total http4xx > 300"; $threshold = 300; $description = "High client errors detected on $resourceShort"; break }
					"CPU" { $name = "HighCPU"; $condition = "avg cpupercentage > 90"; $threshold = 90; $description = "High CPU usage detected on $resourceShort"; break }
					"RAM" { $name = "HighRAMUsage"; $condition = "avg memorypercentage > 85"; $threshold = 85; $description = "High RAM utilization detected on $resourceShort"; break }
					"AGFailures" { $name = "HighFailedRequests"; $condition = "total failedrequests > 30"; $threshold = 30; $description = "High failed requests on $resourceShort"; break }
					"StorageAvailability" {  $name = "Availability"; $condition = "avg availability < 98"; $threshold = 98; $description = "Reduced storage availability on $resourceShort"; break }
				}
			}

			$alertName = "$alertPrefix-$resourceShort-$name"

			#resource FQ
			switch($resource.Keys[0]){
				"AppService" { $resourceFQ = $fqResourceAppService + $resourceShort }
				"AppServicePlan" { $resourceFQ = $fqResourceAppServicePlan + $resourceShort }
			}

			$alert = $alerts | Where-Object { ($_.name -eq $alertName) -and ($_.description -eq $description) -and ($_.criteria.allOf[0].threshold -eq $threshold) }
			if ($alert){
				Write-Host "Monitor alert $alertName is already created"
				continue;
			} 
			Write-Host "Creating monitor alert for $resourceShort $name in $resourceGroup"         
			try{
				az monitor metrics alert create -n $alertName -g $resourceGroup --subscription $subscription --scopes $resourceFQ --condition $condition --action $actionGroups[0] --description $description --evaluation-frequency "5m"
				Write-Host "Successfully created monitor alert for $resourceShort $name in $resourceGroup`r`n"
				for($i=1; $i -lt $actionGroups.Count; $i+=1){
					Write-Host "Adding additional action group to $resourceShort $name in $resourceGroup"
					az monitor metrics alert update -n $alertName -g $resourceGroup --subscription $subscription --scopes $resourceFQ --add-action $actionGroups[$i]
					Write-Host "Successfully updated monitor alert for $resourceShort $name in $resourceGroup`r`n"
				}
			}
			catch{
				Write-Error "Error occurred creating/updating monitor metric alert. See exception"
				Write-Error $_
				continue
			}
			
		}
	}
}

Main