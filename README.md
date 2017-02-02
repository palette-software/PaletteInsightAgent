[![Build status](https://ci.appveyor.com/api/projects/status/reub3y37h4hkw0ck/branch/master?svg=true)](https://ci.appveyor.com/project/palette-software/blackboxrecorder/branch/master)

[![Build Status](https://travis-ci.org/palette-software/PaletteInsightAgent.svg?branch=master)](https://travis-ci.org/palette-software/PaletteInsightAgent)

# Palette Insight Architecture

![Palette Insight Architecture](https://github.com/palette-software/palette-insight/blob/master/insight-system-diagram.png?raw=true)

[Insight Server]: https://github.com/palette-software/insight-server

# Palette Insight Agent

## What is Palette Insight Agent?

Palette Insight Agent does 4 major things:

* collects and parses Tableau Server log files listed in [Config/LogFolders.yml](PaletteInsightAgent/Config/LogFolders.yml)
* collects a set of Windows performance counters configured in [Config/Counters.yml](PaletteInsightAgent/Config/Counters.yml)
* collects CPU consumption data about all the processes, moreover thread-level data for processes configured in [Config/Processes.yml](PaletteInsightAgent/Config/Processes.yml)
* retrieves records from Tableau Server's built in Postgres tables configured in [Config/Repository.yml](PaletteInsightAgent/Config/Repository.yml). **IMPORTANT:** this kind of data collection is only performed on that specific Tableau node where the active Tableau repository can be found.

All the collected data is written into CSV files and the agent sends them to the [Insight Server].

**IMPORTANT NOTE**: the [Insight Server] might not process all the uploaded CSV files. For details please see [LoadTables](https://github.com/palette-software/insight-gp-import), [Reporting](https://github.com/palette-software/insight-reporting-framework) and [Data Model](https://github.com/palette-software/insight-data-model) components.

## How do I set up Palette Insight Agent?

### Prerequisites

* Palette Insight Agent can only be installed with system administrator privileges
* You need to make sure that the Insight Agent is able to query data from the Tableau repository. The easiest way to achieve that is to enable `readonly` user in your Tableau Server, because in that case the the Insight Agent can automatically collect the `readonly` user's password from the `workgroup.yml` file of Tableau Server.

### Installation

Run the Palette Insight Agent installer. During the install you will have to enter your [Insight Server]'s URL.

The installation package contains a [Palette Insight Watchdog](https://github.com/palette-software/palette-updater) service as well, which is responsible for acquiring and applying the available Insight Agent updates from the [Insight Server] and to make sure that the Insight Agent service is running all the time.

#### Alternative configurations

Settings can be manually edited in [Config/Config.yml](PaletteInsightAgent/Config/Config.yml)

* Proxy configurations have to be placed under the `Webservice` key
* In case `readonly` user is not enabled in your Tableau Server, you need to provide Tableau repo credentials manually under the `TableauRepo` key

### Start the Agent

Palette Insight Agent is zero configured and remotely updated.
After installing Palette Insight Agent it will attempt to start automatically within 3 minutes, but you can start it manually from Windows Services panel.

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
