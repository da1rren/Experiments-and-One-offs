$user = [System.Security.Principal.NTAccount]"$env:userdomain\$env:username"
$sid = $user.Translate([System.Security.Principal.SecurityIdentifier]).Value

Write-Host "Hello,  please note you use this script at your own discretion and take full responsibility for what it does.  Press enter if you accept resposibility"
Pause
Write-Host "Getting Key"

$driveKeys = Get-Item -Path Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\

Write-Host "Updating keys"
Set-ItemProperty -path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\" -name NoDrives -value 1

Write-Host "Assuming Direct Control"

$acl = Get-Acl HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\
$acl.SetOwner($user)

Write-Host "Protecting key against unwanted changes"
$ruleUser = New-Object System.Security.AccessControl.RegistryAccessRule ("$env:userdomain\$env:username", "FullControl", "Allow")
$ruleSystem = New-Object System.Security.AccessControl.RegistryAccessRule ("SYSTEM","FullControl", "Deny")
$ruleSystem1 = New-Object System.Security.AccessControl.RegistryAccessRule ("SYSTEM","TakeOwnership", "Deny")
$ruleSystem2 = New-Object System.Security.AccessControl.RegistryAccessRule ("SYSTEM","SetValue", "Deny")
$acl.SetAccessRule($rule)
$acl.SetAccessRule($ruleSystem)
$acl.SetAccessRule($ruleSystem1)
$acl.SetAccessRule($ruleSystem2)
$acl | Set-Acl -Path HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\