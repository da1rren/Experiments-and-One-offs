$ip = Read-Host 'Enter your ip address'

&{$adapter = Get-NetAdapter -Name Ethernet;New-NetIPAddress -InterfaceAlias $adapter.Name -AddressFamily IPv4 -IPAddress $ip -PrefixLength 26 -DefaultGateway 10.243.32.1; Set-DnsClientServerAddress -InterfaceAlias $adapter.Name -ServerAddresses ("10.251.18.96","10.251.18.97")}