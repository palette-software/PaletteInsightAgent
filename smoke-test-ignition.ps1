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
        Write-Host "Let's get ready to rumbleeee!!!"

        # Do the preparations for the test
        (New-Object Net.WebClient).DownloadFile('https://www.cubbyusercontent.com/pl/githubrelease.exe/_80d5198eac2d44b7a31f08060eddd5fe', "$PSScriptRoot\githubrelease.exe")
        #md github-assets
        #cd github-assets
        .\githubrelease.exe palette-software PaletteInsightAgent $env:GITHUB_ACCESS_TOKEN

        $Dir = get-childitem $PSScriptRoot
        $PALIN_MSI = $Dir | where {$_.extension -eq ".msi"}
        $PALIN_MSI | format-table name

        # Do the smoke test
        & "$PSScriptRoot\smoke-test.ps1"

        # Cleanup test
        Remove-Item -Path "C:\Program Files (x86)\Palette Insight Agent" -Recurse


        Write-Host "Aaaaand it's gone."
    } 
}
catch
{
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