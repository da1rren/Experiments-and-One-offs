
#$servers = "Aberchirder2", "Aberlour", "Aboyne2", "Albyn", "Alford", "Ardach", "Auchenblae", "Ballater", "Banchory", "Banff", "Braemar", "Bucksburn", "Bydand", "Camphill", "CBuchan", "Crimond", "Cruden", "Cults", "Cuminestown", "Denburn", "Deveron", "Dufftown", "ElginCom", "Ellon", "FinlaysonSt2", "Fochabers", "ForresHC", "Glenlivet", "Haddo", "MIDDLEFIELD", "Insch", "Inverbervie", "InvGP", "Keith", "Kemnay", "Laurencekirk", "GLASSGREEN", "MacDuff", "Maryhill", "Mintlaw2", "Moraycoast", "Oldmachar", "Oldmeldrum", "Peterculter", "Peterhead", "Portlethen2", "Portsoy", "Rhynie", "Saltoun", "Seafield2", "Stonehaven2", "Strathdon", "Torphins", "Turriff" 

#$servers = "Ballater", "Aberlour"

#Restart-Computer -Force

$servers = "nhsg-int-poctg", "nhsg-sql-poctg"

foreach ($server in $servers) {

    Write-Host "Copying exe to $server"
    Copy-Item -Path "C:\Ict-temp\NDP462-KB3151800-x86-x64-AllOS-ENU.exe" -Destination "\\$server\c$\Ict-temp" -Force

}

$sessions = New-PsSession -ComputerName $servers

Invoke-Command -Session $sessions -ScriptBlock { & 'C:\Ict-temp\NDP462-KB3151800-x86-x64-AllOS-ENU.exe' /q; }

Invoke-Command -Session $sessions -ScriptBlock { Start-Sleep -s 60; while($true){ Start-Sleep -s 5; if(get-process NDP462-KB3151800-x86-x64-AllOS-ENU -ErrorAction SilentlyContinue) {write-host "$env:computername is still running :)"} else { Write-Hostname "Rebooting $env:computername in a bit"; Start-Sleep -s 5; break; };} Write-Host "Restarting $env:computername now"; }