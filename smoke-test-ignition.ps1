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
        # Make sure we are in the right folder
        cd $PSScriptRoot

        # Do the preparations for the test
        Write-Host "Download our Github release downloader tool"
        (New-Object Net.WebClient).DownloadFile('https://www.cubbyusercontent.com/pl/githubrelease.exe/_80d5198eac2d44b7a31f08060eddd5fe', "$PSScriptRoot\githubrelease.exe")
        sleep 2
        & "$PSScriptRoot\githubrelease.exe" palette-software PaletteInsightAgent $env:GITHUB_ACCESS_TOKEN
        sleep 2

        Write-Host "Downloaded .msi files:"
        dir *.msi

        # Store the name of the latest Palette Insight Agent .msi
        $Dir = get-childitem $PSScriptRoot
        $env:PALIN_MSI = $Dir | where {$_.extension -eq ".msi"}
        $env:PALIN_MSI | format-table name

        # Make a sample run of Palette Insight Agent
        Write-Host "Installing $env:PALIN_MSI ..."
        msiexec.exe /qn /i $env:PALIN_MSI
        # HACK: This is so awkward... On GoCD agent execution does not wait for the install to finish... :(
        sleep 3
        Write-Host "Installed Palette Insight Agent successfully"

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
    write-host "$pid : Error caught - $_"

    # Cleanup test
    Write-Host "Cleanup test leftovers"
    msiexec.exe /qn /x $env:PALIN_MSI
    sleep 3
    Write-Host "Uninstalled Palette Insight successfully"
    # Remove-Item -Path "C:\Program Files (x86)\Palette Insight Agent" -Recurse

    # Exit with error code
    if ($? -and (test-path variable:LastExitCode) -and ($LastExitCode -gt 0))
    {
        exit $LastExitCode
    }
    else
    {
        exit 1
    }
}