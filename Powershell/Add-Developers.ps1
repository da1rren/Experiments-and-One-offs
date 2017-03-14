powershell -command "& { ([adsi]'WinNT://./administrators,group').Add('WinNT://nhsg.grampian.scot.nhs.uk/ari it development global,group'); }"
