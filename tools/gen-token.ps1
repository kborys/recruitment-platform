param(
    [Parameter(Mandatory)][string[]]$Role,
    [string]$User = 'dev-user',
    [int]$Ttl = 600,
    [string]$Key = 'dev-signing-key-at-least-32-characters-long-change-me',
    [string]$Issuer = 'recruitment-platform',
    [string]$Audience = 'recruitment-platform'
)

$ErrorActionPreference = 'Stop'

$Role = $Role | ForEach-Object { $_ -split ',' } | Where-Object { $_ }

function B64Url([byte[]]$b) { [Convert]::ToBase64String($b).TrimEnd('=').Replace('+', '-').Replace('/', '_') }

$header = B64Url ([Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))

$roleClaim = if ($Role.Count -eq 1) { $Role[0] } else { @($Role) }
$now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$exp = $now + $Ttl * 60
$payloadJson = [ordered]@{
    sub  = $User
    role = $roleClaim
    iss  = $Issuer
    aud  = $Audience
    nbf  = $now
    exp  = $exp
} | ConvertTo-Json -Compress
$payload = B64Url ([Text.Encoding]::UTF8.GetBytes($payloadJson))

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Key))
try { $sig = B64Url $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$header.$payload")) }
finally { $hmac.Dispose() }

"$header.$payload.$sig"
