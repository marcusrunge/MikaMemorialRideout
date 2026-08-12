Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$randomBytes = [byte[]]::new(24)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($randomBytes)

$adminCode = [System.Convert]::ToBase64String($randomBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')

$salt = [byte[]]::new(32)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($salt)

$codeBytes = [System.Text.Encoding]::UTF8.GetBytes($adminCode)
$hashInput = [byte[]]::new($salt.Length + $codeBytes.Length)

[System.Buffer]::BlockCopy($salt, 0, $hashInput, 0, $salt.Length)
[System.Buffer]::BlockCopy($codeBytes, 0, $hashInput, $salt.Length, $codeBytes.Length)

$hash = [System.Security.Cryptography.SHA256]::HashData($hashInput)

[PSCustomObject]@{
    AdminCode  = $adminCode
    SaltBase64 = [System.Convert]::ToBase64String($salt)
    HashBase64 = [System.Convert]::ToBase64String($hash)
} | Format-List
