function GetKey([System.String]$Verb = '', [System.String]$ResourceId = '',
	[System.String]$ResourceType = '', [System.String]$Date = '', [System.String]$masterKey = '') {
	$keyBytes = [System.Convert]::FromBase64String($masterKey) 
	$text = @($Verb.ToLowerInvariant() + "`n" + $ResourceType.ToLowerInvariant() + "`n" + $ResourceId + "`n" + $Date.ToLowerInvariant() + "`n" + "`n")
	$body = [Text.Encoding]::UTF8.GetBytes($text)
	$hmacsha = new-object -TypeName System.Security.Cryptography.HMACSHA256 -ArgumentList (, $keyBytes) 
	$hash = $hmacsha.ComputeHash($body)
	$signature = [System.Convert]::ToBase64String($hash)
 
	[System.Web.HttpUtility]::UrlEncode($('type=master&ver=1.0&sig=' + $signature))
}

function GetUTDate() {
	$date = get-date
	$date = $date.ToUniversalTime();
	return $date.ToString("ddd, dd MMM yyyy HH:mm:ss \G\M\T")
}
 
function GetDatabases([string]$connectionKey) {
	$uri = $rootUri + "/dbs"
	$hdr = BuildHeaders -resType dbs -connectionKey $connectionKey
	$response = Invoke-RestMethod -Uri $uri -Method Get -Headers $hdr
	$response.Databases
	Write-Host ("Found " + $Response.Databases.Count + " Database(s)")
}
 
function GetCollections([string]$dbname, [string]$connectionKey) {
	$uri = $rootUri + "/" + $dbname + "/colls"
	$headers = BuildHeaders -resType colls -resourceId $dbname -connectionKey $connectionKey
	$response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers
	$response.DocumentCollections
	Write-Host ("Found " + $Response.DocumentCollections.Count + " DocumentCollection(s)")
}
function BuildHeaders([string]$action = "get", [string]$resType, [string]$resourceId, [string]$connectionKey) {
	$authz = GetKey -Verb $action -ResourceType $resType -ResourceId $resourceId -Date $apiDate -masterKey $connectionKey
	$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
	$headers.Add("Authorization", $authz)
	$headers.Add("x-ms-version", '2015-12-16')
	$headers.Add("x-ms-date", $apiDate) 
	$headers.Add("Content-Type", "application/query+json")
	$headers
}
 
function GetDocuments([string]$document, [string]$dbname, [string]$collection, [string]$connectionKey) {
	$collName = "dbs/" + $dbname + "/colls/" + $collection
	$headers = BuildHeaders -action get -resType docs -resourceId $collName -connectionKey $connectionKey
	$uri = $rootUri + "/" + $collName + "/docs"
	$response = Invoke-RestMethod $uri -Method Get -Headers $headers
	$response
}
$apiDate = GetUTDate

function PostDocument([object]$document, [string]$dbname, [string]$collection, [string]$partitionKey, [string]$connectionKey) {
	$collName = "dbs/" + $dbname + "/colls/" + $collection
	$headers = BuildHeaders -action Post -resType docs -resourceId $collName -connectionKey $connectionKey
	$headers.Add("x-ms-documentdb-is-upsert", "true")
	$headers.Add("x-ms-documentdb-partitionkey", '["' + $partitionKey + '"]')
	$uri = $rootUri + "/" + $collName + "/docs"
	$jsonDocument = $document | ConvertTo-Json -Depth 100
	$response = Invoke-RestMethod $uri -Method Post -Body $jsonDocument -ContentType 'application/json' -Headers $headers
	$response
}

function  updateCosmosRecords {
	Param(
		[Parameter(Mandatory = $true)] [string] $resourceGroupName,
		[Parameter(Mandatory = $true)] [string] $collectionName,
		[Parameter(Mandatory = $true)] [string] $folderPath,
		[Parameter(Mandatory = $true)] [array] $excludedProperties,
		[Parameter(Mandatory = $true)] [string] $partitionKey,
		[Parameter(Mandatory = $true)] [string] $accountName,
		[Parameter(Mandatory = $true)] [string] $databaseName
	)
	write-host ("collectionName is " + $collectionName)
        
	$keys = Get-AzCosmosDBAccountKey -ResourceGroupName $resourceGroupName -Name $accountName 
	$connectionKey = $keys.PrimaryMasterKey

	$rootUri = "https://" + $accountName + ".documents.azure.com"
	write-host ("Root URI is " + $rootUri)
 
	$db = GetDatabases -connectionKey $connectionKey | Where-Object { $_.id -eq $databaseName }
 
	if ($null -eq $db) {
		write-error "Could not find database in account"
		return
	} 
 
	$dbname = "dbs/" + $databaseName
	$collection = GetCollections -dbname $dbname -connectionKey $connectionKey | Where-Object { $_.id -eq $collectionName }
    
	if ($null -eq $collection) {
		write-error "Could not find collection in database"
		return
	}
 
	$jsonResponse = GetDocuments -document $json -dbname $databaseName -collection $collectionName -connectionKey $connectionKey
	$jsonFiles = Get-ChildItem $folderPath -Filter "*.json"

	foreach ($file in $jsonFiles) {
		$currentFilePath = "$folderPath$($file[0].Name)"
		$fileObject = (Get-Content -Path $currentFilePath | Out-String | ConvertFrom-Json)
		$fileObjectString = $fileObject | ConvertTo-Json -Compress -Depth 100 | Out-String

		$dbObjectString = $jsonResponse.Documents | Where-Object { $_.id -eq $fileObject.id } | Select-Object -Property * -ExcludeProperty $excludedProperties | ConvertTo-Json -Depth 100 -Compress | Out-String

		$contentEquals = $fileObjectString.Equals($dbObjectString)

		# for connectors check the dynamic case
		if ('ConnectorList' -eq $partitionKey -and -not $contentEquals) {
			if ($fileObject.info.isDynamicFilter -ieq 'True') {
				$dbObject = $jsonResponse.Documents | Where-Object { $_.id -eq $fileObject.id }
				if ($dbObject.PSobject.Properties.Name -contains "filters" -and $dbObject.filters.count -gt 0) {
					$databaseFilters = $dbObject.filters
					[array] $dbObject.filters = $dbObject.filters | Select-Object -Property * -ExcludeProperty 'filterlist'
				}
				if ($fileObject.PSobject.Properties.Name -contains "filters" -and $fileObject.filters.count -gt 0) {
					$fileObjectFilters = $fileObject.filters
					[array] $fileObject.filters = $fileObject.filters | Select-Object -Property * -ExcludeProperty 'filterlist'
				}
				$contentEquals = $fileObject.Equals($dbObject)

				# if we need to push the object makes sure the file has the correct filters.
				if (!$contentEquals) {
					#database has filters
					if ($databaseFilters.filterList.Count -gt 0) {
						$fileObject.filters = $databaseFilters
						Write-Host ("$($fileObject.alias) used filters from database")
					}
					#no filters in the database yet so add in ones from the file
					else {
						$fileObject.filters = $fileObjectFilters
						Write-Host ("$($fileObject.alias) used filters from file")
					}
				}
			}
		}

		# If content doesn't match we need to update cosmos based on what's in Repo
		if (!$contentEquals) {
        
			write-host ("File Json Content - " + $fileObjectString)
			write-host ("DB Json Content   - " + $dbObjectString)

			write-host ("Changes need to be posted to DB for $file")
			PostDocument PostDocument -document $fileObject -dbname $databaseName -collection $collectionName -partitionKey $partitionKey -connectionKey $connectionKey         
		}
		else {
			write-host ("No Change needed for $file")
		}

		write-host ("")
	}
}