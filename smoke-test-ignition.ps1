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

        & "$PSScriptRoot\smoke-test.ps1"

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