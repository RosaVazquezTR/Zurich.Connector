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

    $properties = "ResourceGroupName=$resourceGroupName, RedisResourceName=$redisResourceName, Location=$location, Sku=Standard, Size=C0"

    Write-Host "Creating Azure Cache for Redis $properties..."

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
        Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -AsPlainText
    }

    if ($currentConnectionString -ne $connectionString) {
        Write-Host "Adding connection string to Key Vault..."
        $secret = ConvertTo-SecureString -String $connectionString -AsPlainText -Force
        Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyName -SecretValue $secret
        Write-Host "Connection string successfully added to Key Vault."
    }
    else {
        Write-Host "Connection string already exists in Key Vault."
    }
}

function Get-RedisPrimaryKey {
    param(
        [string]$resourceGroupName,
        [string]$redisResourceName
    )

    $redisKey = Get-RedisKey -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName
    return $redisKey.PrimaryKey
}

# Check if the resource already exists

$context = Get-AzContext

$sp = Get-AzADServicePrincipal -ObjectId $context.Account.Id

# Muestra el nombre del Service Principal
Write-Host "Service Principal Name: $($sp.DisplayName)"

$redisCache = Get-RedisCache -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName

if ($null -eq $redisCache) {
    # If it does not exist, create the resource
    Write-Host "Azure Cache for Redis not found, creating a new one..."
    $redisCache = New-RedisCache -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName -location $location
    Write-Host "Azure Cache for Redis successfully created."
}
else {
    Write-Host "Azure Cache for Redis already exists."
}

$primaryKey = Get-RedisPrimaryKey -resourceGroupName $resourceGroupName -redisResourceName $redisResourceName
$connectionString = $redisCache.HostName + ":6380,password=" + $primaryKey + ",ssl=True,abortConnect=False"

Add-ConnectionStringToKeyVault -keyVaultName $keyVaultName -keyName $keyName -connectionString $connectionString