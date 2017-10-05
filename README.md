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
* retrieves records from Tableau Server's built in Postgres tables configured in [Config/Repository.yml](PaletteInsightAgent/Config/Repository.yml). **IMPORTANT:** this kind of data collection is only performed on that specific Tableau node where the active Tableau repository can be found.

All the collected data is written into CSV files and the agent sends them to the [Insight Server].

**IMPORTANT NOTE**: the [Insight Server] might not process all the uploaded CSV files. For details please see [LoadTables](https://github.com/palette-software/insight-gp-import), [Reporting](https://github.com/palette-software/insight-reporting-framework) and [Data Model](https://github.com/palette-software/insight-data-model) components.

## How do I install Palette Insight Agent?

### Prerequisites

* Palette Insight Agent can only be installed with system administrator privileges
* You need to make sure that the Insight Agent is able to query data from the Tableau repository. The easiest way to achieve that is to enable `readonly` user in your Tableau Server, because in that case the the Insight Agent can automatically collect the `readonly` user's password from the `workgroup.yml` file of Tableau Server.

### Installation
The Palette Insight Agent Windows installer (.msi) can be downloaded from the Palette Insight Server. All you have to do for that is to open your browser and navigate to
[http://your-insight-server-url/control]
and you will be shown a page like this

<img src="https://github.com/palette-software/PaletteInsightAgent/blob/master/docs/resources/insight-server-control-page.png" alt="Insight Server Control Page" width="400" >

In the Agents section you can click on the green button which is showing the agent’s version number. This will initiate the download of the Palette Insight Agent .msi.

Another way to obtain the .msi is from this repo's [Releases](https://github.com/palette-software/PaletteInsightAgent/releases) section.

The installation package contains the `Palette Insight Agent` service and also a [Palette Insight Watchdog](https://github.com/palette-software/palette-updater) service, which is responsible for acquiring and applying the available Insight Agent updates from the [Insight Server] and to make sure that the `Palette Insight Agent` service is running all the time.

During the installation you will need two things:
1. The Insight License Key which was entered to Insight Server’s config file (`/etc/palette-insight-server/server.config`) as the `license_key` value.
1. The IP address or the name of the Insight Server machine (`https://` prefix is required)

And you will have to enter them into this install dialog:
<p align="center">
  <img src="https://raw.githubusercontent.com/palette-software/PaletteInsightAgent/master/docs/resources/insight-install-dialog.png" alt="Insight Agent Install Dialog" width="500" >
</p>

If you leave the fields in this installer dialog as is (i.e. blank field for Insight License Key and `https://` for Insight Server URL), then the values entered into this fields of previous installations will remain in place. You can check these values in `<Palette_Insight_Agent_install_dir>\Config\Config.yml` file.

#### Alternative configurations

Settings can be manually edited in [Config/Config.yml](PaletteInsightAgent/Config/Config.yml)

* Proxy configurations have to be placed under the `Webservice` key
* In case `readonly` user is not enabled in your Tableau Server, you need to provide Tableau repo credentials manually under the `TableauRepo` key


## How do I update Palette Insight Agent?

Palette Insight Agent is updated automatically once a newer version of the .msi is available on the Insight Server to which the agent is connected. On the Insight Server the `palette-insight-agent` RPM package contains the .msi, so that must be installed on the Insight Server and after then the connected Palette Insight Agents will pick up the update in 3-5 minutes.

#### Troubleshooting
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
