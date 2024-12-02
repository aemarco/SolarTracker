Param (
    [string]$USER,
    [string]$TARGET
)
$DEPLOY_DIR = "/home/$USER/solar/"

Write-Host "Stopping the service..."
ssh "$USER@$TARGET" "sudo systemctl stop SolarTracker.service"

Write-Host "Clean target directory..."
ssh "$USER@$TARGET" "rm -r $DEPLOY_DIR/*"

Write-Host "Publishing the application..."
dotnet publish SolarTracker/SolarTracker.csproj -c Release -o SolarTracker\bin\publish -r linux-arm64 -f net9.0 --self-contained true

Write-Host "Copying files to the target machine..."
scp -r SolarTracker/bin/publish/* "$USER@${TARGET}:$DEPLOY_DIR"

Write-Host "Setting executable permission..."
ssh "$USER@$TARGET" "chmod +x $DEPLOY_DIR/SolarTracker"

Write-Host "Creating and enabling the service..."
$serviceFile = @"
[Unit]
Description=SolarTracker
After=network.target
Wants=network.target
Requires=network-online.target

[Service]
WorkingDirectory=$DEPLOY_DIR
ExecStartPre=/bin/sh -c 'until timedatectl status | grep "synchronized: yes"; do sleep 1; done'
ExecStart=sudo $DEPLOY_DIR/SolarTracker
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=solartracker
User=$USER
Group=$USER
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
"@

$serviceFilePath = "/etc/systemd/system/SolarTracker.service"
echo $serviceFile | ssh "$USER@$TARGET" "sudo tee $serviceFilePath > /dev/null"
ssh "$USER@$TARGET" "sudo systemctl daemon-reload"
ssh "$USER@$TARGET" "sudo systemctl enable SolarTracker.service"
ssh "$USER@$TARGET" "sudo systemctl start SolarTracker.service"

Write-Host "Deployment complete!"
