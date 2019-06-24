# Palette Insight Agent Installation Guide

### Prerequisites

Palette Insight Agent can only be installed with _system administrator_ privileges

#### Unique GUID

In order to install the Insight Agent you need a unique identifier which is a standard GUID. You can generate one e.g. at <https://www.guidgenerator.com>.

#### Tableau repository user

You need to make sure that the Insight Agent is able to query data from the Tableau repository. The easiest way to achieve that is to enable `readonly` user in your Tableau Server. You may find the guide [how to enable the readonly user](https://onlinehelp.tableau.com/current/server/en-us/tabadmin_cmd.htm#dbpass) on the Tableau website.

##### Prior Tableau Server 2018.2

The Insight Agent can automatically collect the `readonly` user's password from the `workgroup.yml` file of Tableau Server.

  The necessary `tabadmin` command is:

  ```bash
tabadmin dbpass --username readonly p@ssword
  ```

##### Tableau Server 2018.2 or later

To enable the `readonly` user use the `tsm` command:

  ```bash
tsm data-access repository-access enable --repository-username readonly --repository-password p@ssword
  ```

You will need to enter the credentials of the user into the [Config/Config.yml](PaletteInsightAgent/Config/Config.yml).
This is required even if the `readonly` user is enabled. This should be done on those machines where the active and passive Tableau Repositories are.

  Example:

  ```yaml
  TableauRepo:
   Host: localhost
   Port: 8060
   Database: workgroup
   User: readonly
   Password: p@ssword
  ```

### Installation
The installation package contains the `Palette Insight Agent` service and also a [Palette Insight Watchdog](https://github.com/palette-software/palette-updater) service, which is to make sure that the `Palette Insight Agent` service is running all the time.

During the installation you will need two things:
1. The Insight License Key is the GUID you generated before and which was entered to Insight Server’s config file (`/etc/palette-insight-server/server.config`) as the `license_key` value.
1. The IP address or the hostname of the Insight Server machine (`https://` prefix is required)

And you will have to enter them into this install dialog:
<p align="center">
  <img src="https://raw.githubusercontent.com/palette-software/PaletteInsightAgent/master/docs/resources/insight-install-dialog.png" alt="Insight Agent Install Dialog" width="500" >
</p>

If you leave the fields in this installer dialog as is (i.e. blank field for Insight License Key and `https://` for Insight Server URL), then the values entered into this fields of previous installations will remain in place. You can check these values in `<Palette_Insight_Agent_install_dir>\Config\Config.yml` file.

#### Alternative configurations

Settings can be manually edited in [Config/Config.yml](PaletteInsightAgent/Config/Config.yml)

* Proxy configurations have to be placed under the `Webservice` key
* In case `readonly` user is not enabled in your Tableau Server, you need to provide Tableau repo credentials manually under the `TableauRepo` key
