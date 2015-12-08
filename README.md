# PalMon #

## What is PalMon?

PalMon is a performance monitoring agent that periodically samples target hosts for a set of Perfmon and MBean counters and writes out the results to a database in a Tableau-friendly format.  This information can then be used to monitor & analyze performance of a Tableau Server installation, in order to detect potential issues or assess scalabiliy & sizing.

PalMon can be run as both a console app and a Windows service.

## How do I set up PalMon?

PalMon is deployed via a custom installer, which manages dependencies and also bundles Postgres for ease of setup for new users.  To get up and running, follow the instructions in the [installation guide](https://github.com/tableau/PalMon/blob/master/PalMonService/Documentation/UserGuide.pdf).

## How do I analyze results from PalMon?

The best way is to explore your results in Tableau!  A [sample workbook](https://github.com/tableau/PalMon/blob/master/Sample%20Workbooks/PalMon%20Workbook.twb) has been created with some example dashboards & views to get you started.

## What do I need to build PalMon from source?

The current development requirements are:

1. Windows operating system.
2. Visual Studio 2013 or later.
3. WiX Toolset Visual Studio Extension - Required to build the installer projects.
  * Available at http://www.wixtoolset.org
4. [Java SE Development Kit](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html)

See [Developer Notes.txt](https://github.com/tableau/PalMon/blob/master/Developer%20Notes.txt) for additional developer-specific notes.

## Is PalMon supported?

PalMon is made available AS-IS with no support. This is intended to be a self service tool and includes a user guide.  Any bugs discovered should be filed in the [PalMon Git issue tracker](https://github.com/tableau/PalMon/issues).

## How can I contribute to PalMon?

Code contributions & improvements by the community are welcomed & encouraged!  See [the LICENSE file](https://github.com/tableau/PalMon/blob/master/LICENSE) for current open-source licensing & use information.
