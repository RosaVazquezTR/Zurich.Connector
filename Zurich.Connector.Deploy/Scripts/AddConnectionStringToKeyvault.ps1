param(
	[Parameter(Mandatory = $true)]
	[string]     $resourceGroup,

	[Parameter(Mandatory = $true)]
	[string]     $vaultName,

    [Parameter(Mandatory = $true)]
	[string]     $databaseServerName,

    [Parameter(Mandatory = $true)]
	[string]     $databaseName,

    [Parameter(Mandatory = $true)]
	[string]     $databaseUserName,

    [Parameter(Mandatory = $true)]
	[string]     $databaseUserPassword,

	[Parameter(Mandatory = $true)]
	[string]     $environment,

    [Parameter(Mandatory = $true)]
	[string]     $keyName,

    [Parameter(Mandatory = $true)]
	[string]     $alwaysEncrypted
)

function Retry($action) {
	$attempts = 5
	$sleepInSeconds = 10

	do {
		try {
			return $action.Invoke();
		}
		catch [Exception] {
			if ($attempts -eq 1) {
				throw
			}

			Write-Host $_.Exception.Message
		}
		$attempts--
		if ($attempts -gt 0) { sleep $sleepInSeconds }
	} while ($attempts -gt 0)

	return null; 
}

function PopulateKeyVaultEncryptedDBConnectionString {
	param(
		[Parameter(Mandatory = $true)]
		[string]     $resourceGroup,
 
		[Parameter(Mandatory = $true)]
		[string]     $vaultName,

        [Parameter(Mandatory = $true)]
		[string]     $databaseServerName,

        [Parameter(Mandatory = $true)]
		[string]     $databaseName,

        [Parameter(Mandatory = $true)]
		[string]     $databaseUserName,

        [Parameter(Mandatory = $true)]
		[string]     $databaseUserPassword,

        [Parameter(Mandatory = $true)]
        [string]     $keyName,
    
        [Parameter(Mandatory = $true)]
        [string]     $alwaysEncrypted
	)

	#Get Keys
	#Parameterize the namespace, hub name, and AuthorizationRule name
	Write-Host "Getting current connection string to check if we need to change KeyVault for $($keyName)"
	$databaseConnectionString = "Server=tcp:$($databaseServerName).database.windows.net,1433;Initial Catalog=$($databaseName);Persist Security Info=False;User ID=$($databaseUserName);Password=$($databaseUserPassword);MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    if ("true" -eq $alwaysEncrypted) {
        $databaseConnectionString = $databaseConnectionString + "Column Encryption Setting=Enabled;"
    }

	Write-Host "Getting connection string that exists in keyvault for $($keyName)"
	$currentDatabaseConnectionString = Retry( { Get-AzKeyVaultSecret -VaultName $vaultName -Name $keyName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue })

	if ($currentDatabaseConnectionString.SecretValueText -ne $databaseConnectionString) {
		Write-Host "Changing connection string in KeyVault for $($keyName)"
		Retry( { Set-AzKeyVaultSecret -VaultName $vaultName -Name $keyName -SecretValue (ConvertTo-SecureString -String $databaseConnectionString -AsPlainText -Force) })
	}
}

$databaseServerName = $databaseServerName + $environment

Write-Host "Populating keyvault connection string for the tenant database"
PopulateKeyVaultEncryptedDBConnectionString -resourceGroup $resourceGroup -vaultName $vaultName -databaseServerName $databaseServerName -databaseName $databaseName -databaseUserName $databaseUserName -databaseUserPassword $databaseUserPassword -keyName $keyName -alwaysEncrypted $alwaysEncrypted
