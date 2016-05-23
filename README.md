[![Build status](https://ci.appveyor.com/api/projects/status/reub3y37h4hkw0ck/branch/master?svg=true)](https://ci.appveyor.com/project/palette-software/blackboxrecorder/branch/master)

# Palette Insight Agent #

## What is Palette Insight Agent?

Palette Insight Agent is a performance monitoring agent that periodically samples target hosts for a set of Perfmon and MBean counters and writes out the results to a database in a Tableau-friendly format.  This information can then be used to monitor & analyze performance of a Tableau Server installation, in order to detect potential issues or assess scalabiliy & sizing.

Palette Insight Agent can be run as both a console app and a Windows service.

## How do I set up Palette Insight Agent?

Palette Insight Agent is zero configured and remotely updated.

## How do I analyze results from Palette Insight Agent?

The best way is to explore your results in Tableau!  A [sample workbook](https://github.com/palette-software/BlackBoxRecorder/blob/master/Sample%20Workbooks/PaletteInsightAgent%20Workbook.twb) has been created with some example dashboards & views to get you started.

## What do I need to build Palette Insight Agent from source?

The current development requirements are:

1. Windows operating system.
2. Visual Studio 2013 or later.
3. WiX Toolset Visual Studio Extension - Required to build the installer projects.
  * Available at http://www.wixtoolset.org
4. [Java SE Development Kit](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html)
5. [Our LogEntries fork](https://github.com/palette-software/le_dotnet)'s NuGet feed ([here](https://www.appveyor.com/docs/nuget#configuring-private-nuget-feed-in-visual-studio) is how you can add the NuGet feed as a source):  
   https://ci.appveyor.com/nuget/le-dotnet-hslfeubjh9oe


See [Developer Notes.txt](https://github.com/palette-software/BlackBoxRecorder/blob/master/Developer%20Notes.txt) for additional developer-specific notes.

## Is Palette Insight Agent supported?

Palette Insight Agent is made available AS-IS with licensed support. Any bugs discovered should be filed in the [Palette Insight Agent Git issue tracker](https://github.com/palette-software/BlackBoxRecorder/issues).

## How can I contribute to Palette Insight Agent?

Code contributions & improvements by the community are welcomed & encouraged!  See [the LICENSE file](https://github.com/palette-software/BlackBoxRecorder/blob/master/LICENSE) for current open-source licensing & use information.
