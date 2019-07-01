[![Build status](https://ci.appveyor.com/api/projects/status/reub3y37h4hkw0ck/branch/master?svg=true)](https://ci.appveyor.com/project/palette-software/blackboxrecorder/branch/master)

[![Build Status](https://travis-ci.org/palette-software/PaletteInsightAgent.svg?branch=master)](https://travis-ci.org/palette-software/PaletteInsightAgent)

# Palette Insight Agent

Palette Insight Agent: Tableau Server monitoring component for collecting data about Tableau Server utilization

For help with installation or configuration of this product contact info@brilliant-data.com or go to www.brilliant-data.com.

# Palette Insight Architecture

![Palette Insight Architecture](https://github.com/palette-software/palette-insight/blob/master/insight-system-diagram.png?raw=true)

[Insight Server]: https://github.com/palette-software/insight-server


## What is Palette Insight Agent?

Palette Insight Agent does 4 major things:

* collects and parses Tableau Server log files listed in [Config/LogFolders.yml](PaletteInsightAgent/Config/LogFolders.yml)
* collects a set of Windows performance counters configured in [Config/Counters.yml](PaletteInsightAgent/Config/Counters.yml)
* collects CPU consumption data about all the processes, moreover thread-level data for processes configured in [Config/Processes.yml](PaletteInsightAgent/Config/Processes.yml)
* retrieves records from Tableau Server's built in Postgres tables configured in [Config/Repository.yml](PaletteInsightAgent/Config/Repository.yml). **IMPORTANT:** this kind of data collection is only performed on that specific Tableau node where the target Tableau repository can be found (passive repository by default).

All the collected data is written into CSV files and the agent sends them to the [Insight Server].

**IMPORTANT NOTE**: the [Insight Server] might not process all the uploaded CSV files. For details please see [LoadTables](https://github.com/palette-software/insight-gp-import), [Reporting](https://github.com/palette-software/insight-reporting-framework) and [Data Model](https://github.com/palette-software/insight-data-model) components.

## How do I install Palette Insight Agent?

You need to copy the MSI installation package to the Tableau Server nodes, execute them and follow the installation wizard. For details please see the [INSTALL.md](INSTALL.md).


## How do I update Palette Insight Agent?

Palette Insight Agent is updated automatically once a newer version of the .msi is available on the Insight Server to which the agent is connected. On the Insight Server the `palette-insight-agent` RPM package contains the .msi, so that must be installed on the Insight Server and after then the connected Palette Insight Agents will pick up the update in 3-5 minutes.

### Avoid Tableau restart

Because of the way MSI installer works it is adviced to stop both the  `PaletteInsightAgent` and the `PaletteInsightWatchdog` services before installing the new version. Otherwise the installer might suggest restarting the machine for the changes to be applied.

Neither the Tableau Server node nor the Tableau Server service should ever be restarted when you make changes to the Palette Insight Agent.

## Troubleshooting

Auto-update may fail, if any of the following circumstances applies on the Tableau Server machine while the auto-update is being performed:
<a name="update-obstacles"></a>
* SysInternals’ Process Explorer is running (this one is the most likely to prevent the service to be removed, and putting it into a Disabled state)
* Task Manager is opened.
* Microsoft Management Console (MMC) is opened. To ensure all instances are closed, run taskkill /F /IM mmc.exe.
* Services console is opened. This is the same as the previous point, since Services console is hosted by MMC.
* Event Viewer is opened. Again, this is the same as the first point.
* In the registry, any of the following keys exists:
  * `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PaletteInsightAgent`
  * `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PaletteInsightWatchdog`
* Someone else is logged into the server and has one of the previously mentioned applications opened.
* An instance of Visual Studio used to debug the service is open.

In case when the auto-update fails, Palette Insight Agent might end up in a situation where it’s executables and binary files are removed, but its services (`Palette Insight Agent` and `Palette Insight Watchdog`) in the Services console still exist. In such cases Palette Insight Agent needs to be updated manually.

First try to remove them with opening up a Command Prompt with Administrator privileges and run the following commands:

```
sc delete PaletteInsightAgent
sc delete PaletteInsightWatchdog
```

If any of the above commands results in a message like this

```
The specified service has been marked for deletion.
```

it means that the given service got in Disabled state. You can verify that in Services console, for example you could see in there something like this:
<p align="center">
  <img src="https://raw.githubusercontent.com/palette-software/PaletteInsightAgent/master/docs/resources/disabled-service.png" alt="disabled Palette Insight Watchdog service" width="800">
</p>

To remedy this situation you need to close those applications which can prevent Windows services to be uninstalled (listed [here](#update-obstacles)).

If you check and refresh the Services console again, the disabled services should have been disappeared, once you have closed those applications listed [above](#update-obstacles).

In very rare cases, if any of `Palette Insight Agent` or `Palette Insight Watchdog` services still remains in Disabled state, you need to restart your machine to have those services disappear from the Services console.

Until Palette Insight Agent or Palette Insight Watchdog services are still displayed in the Services console, you cannot manually re-install Palette Insight Agent.

## Starting the Agent

Palette Insight Agent is zero configured and remotely updated.
After installing Palette Insight Agent it will attempt to start automatically within 3 minutes, but you can start it manually from Services console.

## What do I need to build Palette Insight Agent from source?

The current development requirements are:

1. Windows operating system.
2. Visual Studio 2013 or later.
3. WiX Toolset Visual Studio Extension - Required to build the installer projects.
    * Available at <http://www.wixtoolset.org>

See [Developer Notes.txt](PaletteInsightAgent/blob/master/Developer%20Notes.txt) for additional developer-specific notes.

## How can I test-drive Palette Insight Agent?

Install Palette Insight Agent on all machines of your Tableau cluster. Make sure that there is some traffic on your Tableau Server by logging in and viewing and editing workbooks for example.

Moreover you can find a smoke test run in [appveyor.yml](appveyor.yml), which runs on every build performed by the AppVeyor build environment. It can be useful to check it out in order to have a basic understanding about the data flow around Palette Insight Agent. It also contains some basic interoperability checks with the latest [Insight Server].

## Is Palette Insight Agent supported?

Palette Insight Agent is licensed under GNU GPL. For professional support please contact <developers@palette-software.com>.
Any bugs discovered should be filed in the [Palette Insight Agent Git issue tracker](https://github.com/palette-software/PaletteInsightAgent/issues) or contribution is more than welcome.
