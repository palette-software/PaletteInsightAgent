# Palette Insight Agent #

## What is Palette Insight Agent?

Palette Insight Agent is a performance monitoring agent that periodically samples target hosts for a set of Perfmon and MBean counters and writes out the results to a database in a Tableau-friendly format.  This information can then be used to monitor & analyze performance of a Tableau Server installation, in order to detect potential issues or assess scalabiliy & sizing.

Palette Insight Agent can be run as both a console app and a Windows service.

## How do I set up Palette Insight Agent?

Palette Insight Agent is deployed via a custom installer, which manages dependencies and also bundles Postgres for ease of setup for new users.  To get up and running, follow the instructions in the [installation guide](https://github.com/palette-software/BlackBoxRecorder/blob/master/PaletteInsightAgentService/Documentation/UserGuide.pdf).

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
6. [TypeMock Isolator for C#](http://www.typemock.com/files/TypemockIsolatorSuite-8.2.3.20.msi). We already have developer licenses:  
    Typemock Isolator - Essential  Edition (6 ) License  
    ................................................................................  
    Order#                  :  ORD-04292-F3T2Y3  
    Subscription expires:  04-Apr-2016  
    Company               :  Starschema  
    ................................................................................  
    License Key            : M9CC-FE5N-CB62-2F44-7B4A  
    ................................................................................  

   and 1 [build server] license:  
    Typemock Isolator - Essential Build Server  Edition (1 ) License  
    ................................................................................  
    Order#                  :  ORD-04292-F3T2Y3  
    Subscription expires:  04-Apr-2016  
    Company               :  Starschema  
    ................................................................................  
    License Key            : DF7E-AE4N-7BAB-EFC4-463B  
    ................................................................................  


See [Developer Notes.txt](https://github.com/palette-software/BlackBoxRecorder/blob/master/Developer%20Notes.txt) for additional developer-specific notes.

## Is Palette Insight Agent supported?

Palette Insight Agent is made available AS-IS with no support. This is intended to be a self service tool and includes a user guide.  Any bugs discovered should be filed in the [Palette Insight Agent Git issue tracker](https://github.com/palette-software/BlackBoxRecorder/issues).

## How can I contribute to Palette Insight Agent?

Code contributions & improvements by the community are welcomed & encouraged!  See [the LICENSE file](https://github.com/palette-software/BlackBoxRecorder/blob/master/LICENSE) for current open-source licensing & use information.
