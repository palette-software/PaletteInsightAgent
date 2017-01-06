[![Build status](https://ci.appveyor.com/api/projects/status/reub3y37h4hkw0ck/branch/master?svg=true)](https://ci.appveyor.com/project/palette-software/blackboxrecorder/branch/master)

# Palette Insight Architecture

![GitHub Logo](https://github.com/palette-software/palette-insight/blob/master/insight-system-diagram.png?raw=true)

# Palette Insight Agent #

## What is Palette Insight Agent?

Palette Insight Agent does 4 major things:
* collects and parses Tableau Server log files listed in [Config/LogFolders.yml](PaletteInsightAgent/Config/LogFolders.yml)
* collects a set of Windows performance counters configured in [Config/Counters.yml](PaletteInsightAgent/Config/Counters.yml)
* collects CPU consumption data about all the processes, moreover thread-level data for processes configured in [Config/Processes.yml](PaletteInsightAgent/Config/Processes.yml)
* retrieves records from Tableau Server's built in Postgres tables configured in [Config/Repository.yml](PaletteInsightAgent/Config/Repository.yml). **IMPORTANT:** this kind of data collection is only performed on that specific Tableau node where the active Tableau repository can be found.

All the collected data is written into CSV files and the agent sends them to the [Insight Server](https://github.com/palette-software/insight-server).

**IMPORTANT NOTE**: the Insight Server might not process all the uploaded CSV files. For details please see [LoadTables](https://github.com/palette-software/insight-gp-import), [Reporting](https://github.com/palette-software/insight-reporting-framework) and [Data Model](https://github.com/palette-software/insight-data-model) components.

## How do I set up Palette Insight Agent?

### Prerequisites
* Palette Insight Agent can only be installed with system adminsitrator privileges
**(TODO: Check run as user)**
* You need to make sure that the Insight Agent is able to query data from the Tableau repository. The easiest way to achive that is to enable `readonly` user in your Tableau Server, because in that case the the Insight Agent can automatically collect the `readonly` user's password from the `workgroup.yml` file of Tableau Server.

### Installation
Run the Palette Insight Agent installer. During the install you will have to enter your Insight Server's URL.
**(TODO: Remove the license text field from the installer!!!)**

#### Alternative configurations
Settings can be manually edited in [Config/Config.yml](PaletteInsightAgent/Config/Config.yml)
* Proxy configuration
* In case `readonly` user is not enabled in your Tableau Server, you need to provide Tableau repo credentials manually.

### Start the Agent
Palette Insight Agent is zero configured and remotely updated.
After installing Palette Insight Agent it will attempt to start automatically within 3 minutes, but you can start it manually from Windows Services panel.

## What do I need to build Palette Insight Agent from source?

The current development requirements are:

1. Windows operating system.
2. Visual Studio 2013 or later.
3. WiX Toolset Visual Studio Extension - Required to build the installer projects.
  * Available at http://www.wixtoolset.org
4. [Java SE Development Kit](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html)
5. [Our LogEntries fork](https://github.com/palette-software/le_dotnet)'s NuGet feed ([here](https://www.appveyor.com/docs/nuget#configuring-private-nuget-feed-in-visual-studio) is how you can add the NuGet feed as a source):  
   https://ci.appveyor.com/nuget/le-dotnet-hslfeubjh9oe


See [Developer Notes.txt](https://github.com/palette-software/PaletteInsightAgent/blob/master/Developer%20Notes.txt) for additional developer-specific notes.

## Is Palette Insight Agent supported?

Palette Insight Agent is made available AS-IS with licensed support. Any bugs discovered should be filed in the [Palette Insight Agent Git issue tracker](https://github.com/palette-software/PaletteInsightAgent/issues).

