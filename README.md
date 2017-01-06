[![Build status](https://ci.appveyor.com/api/projects/status/reub3y37h4hkw0ck/branch/master?svg=true)](https://ci.appveyor.com/project/palette-software/blackboxrecorder/branch/master)

# Palette Insight Architecture

![GitHub Logo](https://github.com/palette-software/palette-insight/blob/master/insight-system-diagram.png?raw=true)

# Palette Insight Agent #

## What is Palette Insight Agent?

Palette Insight Agent does 4 major things:
* collects and parses Tableau Server log files listed in [Config/LogFolders.yml](PaletteInsightAgent/Config/LogFolders.yml)
* collects a set of Windows performance counters configured in [Config/Counters.yml](PaletteInsightAgent/Config/Counters.yml)
* collects CPU consumption data about all the processes, moreover thread-level data for processes configured in [Config/Processes.yml](PaletteInsightAgent/Config/Processes.yml]
* retrieves records from Tableau Server's built in Postgres tables configured in [Config/Repository.yml](PaletteInsightAgent/Config/Repository.yml)

All the collected data is written into CSV files and the agent sends them to the [Insight Server](https://github.com/palette-software/insight-server).

IMPORTANT NOTE: the Insight Server might not process all the uploaded CSV files. For details please see [LoadTables](https://github.com/palette-software/insight-gp-import), [Reporting](https://github.com/palette-software/insight-reporting-framework) and [Data Model](https://github.com/palette-software/insight-data-model) components.

## How do I set up Palette Insight Agent?

Palette Insight Agent is zero configured and remotely updated.

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

