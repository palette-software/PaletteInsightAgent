set-strictmode -version latest
$ErrorActionPreference = 'Stop'

function execute-externaltool
(
    [string] $context,
    [scriptblock] $actionBlock
)
{
    # This function exists to check the exit code for the external tool called within the script block, so we don't have to do this for each call
    $LastExitCode = 0;
    & $actionBlock
    if ($LastExitCode -gt 0) { throw "$context : External tool call failed" }
}


try
{
    write-host "Script:            " $MyInvocation.MyCommand.Path
    write-host "Pid:               " $pid
    write-host "Host.Version:      " $host.version
    write-host "Execution policy:  " $(get-executionpolicy)

    # Launch the smoke test
    execute-externaltool "Launch smoke test" {
        # Do the preparations for the test
        Write-Host "Download our Github release downloader tool"
        (New-Object Net.WebClient).DownloadFile('https://www.cubbyusercontent.com/pl/githubrelease.exe/_80d5198eac2d44b7a31f08060eddd5fe', "$PSScriptRoot\githubrelease.exe")
        & "$PSScriptRoot\githubrelease.exe" palette-software PaletteInsightAgent $env:GITHUB_ACCESS_TOKEN

        # Store the name of the latest Palette Insight Agent .msi
        $Dir = get-childitem $PSScriptRoot
        $PALIN_MSI = $Dir | where {$_.extension -eq ".msi"}
        $PALIN_MSI | format-table name

        # Setup the target database credentials
        $env:PGUSER="palette"
        $env:PGPASSWORD="L0fasz1234"
        psql -h 52.90.169.216 -d paletterobot -c "DROP TABLE countersamples;"
        psql -h 52.90.169.216 -d paletterobot -c "DROP TABLE threadinfo;"
        psql -h 52.90.169.216 -d paletterobot -c "DROP TABLE serverlogs;"
        psql -h 52.90.169.216 -d paletterobot -c "DROP TABLE filter_state_audit;"

        # Do the smoke test
        Write-Host "Launching smoke test"
        & "$PSScriptRoot\smoke-test.ps1"

        # Cleanup test
        Write-Host "Cleanup test leftovers"
        Remove-Item -Path "C:\Program Files (x86)\Palette Insight Agent" -Recurse

        Write-Host "Smoke test finished successfully."
    } 
}
catch
{
    # Cleanup test
    Write-Host "Cleanup test leftovers"
    msiexec.exe /qn /x $PALIN_MSI
    sleep 3
    Write-Host "Uninstalled Palette Insight successfully"
    # Remove-Item -Path "C:\Program Files (x86)\Palette Insight Agent" -Recurse


    write-host "$pid : Error caught - $_"
    if ($? -and (test-path variable:LastExitCode) -and ($LastExitCode -gt 0))
    {
        exit $LastExitCode
    }
    else
    {
        exit 1
    }
}