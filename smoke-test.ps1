# Make sure we are in the right folder
cd $PSScriptRoot

# Prepare tables in the target database
$env:PGUSER="palette"
$env:PGPASSWORD="L0fasz1234"
psql -h 52.90.169.216 -d paletterobot -f create_tables.sql
Write-Host "Postgres database setup completed"

# Set the Postgres port in Config.yml
$watched_folder_1 = "c:\watched_folder_1";
$watched_folder_2 = "c:\watched_folder_2";
Remove-Item -Path $watched_folder_1 -Recurse
Remove-Item -Path $watched_folder_2 -Recurse
md $watched_folder_1;
md $watched_folder_2;
$env:CONFIG_YML_PATH = "C:\Program Files (x86)\Palette Insight Agent\Config\Config.yml";
copy "$PSScriptRoot/test/configs/Config_GoCD.yml" $env:CONFIG_YML_PATH
echo " - Name: PaletteInsightAgent" > "C:\Program Files (x86)\Palette Insight Agent\Config\Processes.yml";
copy "$PSScriptRoot\PaletteInsightAgent\debug.license" "c:\Program Files (x86)\Palette Insight Agent"
Write-Host "Reconfigured config.yml"

(New-Object Net.WebClient).DownloadFile('https://github.com/palette-software/insight-tester/releases/download/v0.5-lw/windows_amd64.zip', "$PSScriptRoot\insight-tester.zip")
Write-Host "Downloaded insight-tester"
Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory("$PSScriptRoot\insight-tester.zip", "$PSScriptRoot");
Write-Host "Unzipped insight tester"
(New-Object Net.WebClient).DownloadFile('https://www.cubbyusercontent.com/pl/example_serverlogs.zip/_dff3cc9075aa4c9f8be14c4aeeb0f734', "$PSScriptRoot\example_serverlogs.zip")
Write-Host "Downloaded example_serverlogs.zip"
Write-Host "Unzipped insight tester"
& "$PSScriptRoot\windows_amd64\agentrunner.exe" start
Write-Host "Started PaletteInsightAgent service"
sleep 5
Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory("$PSScriptRoot\example_serverlogs.zip", $watched_folder_1);
sleep 25
Write-Host "Sleeping for 30 seconds"
& "$PSScriptRoot\windows_amd64\agentrunner.exe" stop
Write-Host "Contents of the PaletteInsightAgent log"
type "C:\Program Files (x86)\Palette Insight Agent\Logs\PaletteInsightAgent.nlog.txt"
Write-Host "End of PaletteInsightAgent log"

If (Select-String -Pattern 'ERROR','FATAL' -CaseSensitive -SimpleMatch -Path "C:\Program Files (x86)\Palette Insight Agent\Logs\PaletteInsightAgent.nlog.txt") {
    Write-Host "ERROR or FATAL found in log. Exiting with code 1" 
    exit 1
}

Write-Host "Checking DB contents"
(New-Object Net.WebClient).DownloadFile('https://github.com/palette-software/insight-tester/raw/master/appveyor_tests.json', "$PSScriptRoot\appveyor_tests.json")
& "$PSScriptRoot\windows_amd64\dbcheck.exe" appveyor_tests.json "C:\Program Files (x86)\Palette Insight Agent\Config\Config.yml"
msiexec.exe /qn /x $env:PALIN_MSI
Write-Host "Uninstalled Palette Insight successfully"