param(
    [string]$resourceGroupName,
    [string]$redisResourceName,
    [string]$location,
    [string]$keyVaultName,
    [string]$keyName
)

# Function to retry an action
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
        if ($attempts -gt 0) { Start-Sleep $sleepInSeconds }
    } while ($attempts -gt 0)

    return null; 
}

# Function to check if the Azure Cache for Redis already exists
function Get-RedisCache {
    param(
        [string]$resourceGroupName,
        [string]$redisResourceName
    )

    $redisCache = Get-AzRedisCache -ResourceGroupName $resourceGroupName -Name $redisResourceName -ErrorAction SilentlyContinue
    return $redisCache
}

# Function to get the Redis key
function Get-RedisKey {
    param(
        [string]$resourceGroupName,
        [string]$redisResourceName
    )

    $redisKey = Get-AzRedisCacheKey -ResourceGroupName $resourceGroupName -Name $redisResourceName
    return $redisKey
}

# Function to create a new Azure Cache for Redis
function New-RedisCache {
    param(
        [string]$resourceGroupName,
        [string]$redisResourceName,
        [string]$location
    )

    $newRedisCache = New-AzRedisCache -ResourceGroupName $resourceGroupName -Name $redisResourceName -Location $location -Sku Standard -Size C0
    return $newRedisCache
}

# Function to add the connection string to the Key Vault
function Add-ConnectionStringToKeyVault {
    param(
        [string]$keyVaultName,
        [string]$keyName,
        [string]$connectionString
    )
    
    $currentConnectionString = Retry {
        Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
    
    if ($currentConnectionString.SecretValueText -ne $connectionString) {
        Write-Host "Adding connection string to Key Vault..."
        $secret = ConvertTo-SecureString -String $connectionString -AsPlainText -Force
        Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyName -SecretValue $secret
        Write-Host "Connection string successfully added to Key Vault."
    }
    else {
        Write-Host "Connection string already exists in Key Vault."
    }
}

# Check if the resource already exists

$redisCache = Get-RedisCache -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName

if ($null -eq $redisCache) {
    # If it does not exist, create the resource
    Write-Host "Azure Cache for Redis not found, creating a new one..."
    $redisCache = New-RedisCache -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName -location $location
    Write-Host "Azure Cache for Redis successfully created."
}

Write-Host "Azure Cache for Redis already exists. Connection string:"
$connectionString = $redisCache.HostName + ":6380,password=" + (Get-RedisKey -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName).PrimaryKey + ",ssl=True,abortConnect=False"
Write-Host $connectionString

Add-ConnectionStringToKeyVault -keyVaultName $keyVaultName -keyName $keyName -connectionString $connectionString