$servers = 'DEV-IIS-01', 'DEV-IIS-02'
$sessions = New-PsSession -ComputerName $servers

Invoke-Command -Session $sessions -ScriptBlock {Install-WindowsFeature Web-Server; Install-WindowsFeature Web-Asp-Net; Install-WindowsFeature Web-Asp-Net45; Install-WindowsFeature Web-WebSockets; Install-WindowsFeature Web-Http-Redirect; Install-WindowsFeature Web-Request-Monitor; Install-WindowsFeature Web-Http-Tracing; Install-WindowsFeature Web-Windows-Auth; Install-WindowsFeature Web-Basic-Auth; Install-WindowsFeature Web-Scripting-Tools; Install-WindowsFeature Web-Mgmt-Service}

Invoke-command -Session $sessions -ScriptBlock {Set-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\WebManagement\Server -Name EnableRemoteManagement -Value 1}
Invoke-command -Session $sessions -ScriptBlock {Set-Service -name WMSVC -Startuptype Automatic}
Invoke-command -Session $sessions -ScriptBlock {start-service wmsvc}
Invoke-command -Session $sessions -ScriptBlock {shutdown /r /t 0}