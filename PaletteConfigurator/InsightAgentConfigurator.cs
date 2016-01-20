﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using PaletteConfigurator.PalMonConf;

namespace PaletteConfigurator
{
    public partial class InsightAgentConfigurator : UserControl
    {

        // Disable the Designer to serialize this field
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public InsightAgentConfiguration Config
        {
            get { return config; }
            set
            {
                config = value;
                configBindingSource.DataSource = value;
            }
        }

        private const string ConfigFileName = "PalMon.Config";
        private InsightAgentConfiguration config;

        public InsightAgentConfigurator()
        {
            InitializeComponent();

            InitializeConfiguration();
            SetupBindings();

        }

        private void SetupBindings()
        {
            agentLocationSelector.DataBindings.Add(new Binding("SelectedFolder", configBindingSource, "AgentFolder", true, DataSourceUpdateMode.OnPropertyChanged));

            logfilesFolderSelector.DataBindings.Add(new Binding("SelectedFolder", configBindingSource, "LogWatchFolder", true, DataSourceUpdateMode.OnPropertyChanged));

            logFileMaskText.DataBindings.Add(new Binding("Text", configBindingSource, "LogWatchMask", true, DataSourceUpdateMode.OnPropertyChanged));

            resultDbConfiguration.DataBindings.Add(new Binding("Database", configBindingSource, "ResultsDatabase", true, DataSourceUpdateMode.OnPropertyChanged));

            tableauRepo.DataBindings.Add(new Binding("Database", configBindingSource, "TableauRepo", true, DataSourceUpdateMode.OnPropertyChanged));


            threadPollNumeric.DataBindings.Add(new Binding("Value", configBindingSource, "ThreadInfoPollInterval"));
            pollIntervalNumeric.DataBindings.Add(new Binding("Value", configBindingSource, "PollInterval"));
            logPollNumeric.DataBindings.Add(new Binding("Value", configBindingSource, "LogPollInterval"));
        }

        private void InitializeConfiguration()
        {
            Config = new InsightAgentConfiguration
            {
                PollInterval = 30,
                LogPollInterval = 300,
                ThreadInfoPollInterval = 15,

                // Set the agent folder to the currently running programs directory
                AgentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),

                ResultsDatabase = new DbDetails
                {
                    DbType = "Postgres",
                    Host = "127.0.0.1",
                    Port = 5432,
                    Username = "palette",
                    Password = "palette123",
                    Database = "palette_insight"
                },

                TableauRepo = new DbDetails
                {
                    DbType = "Postgres",
                    Host = "127.0.0.1",
                    Port = 8060,
                    Username = "readonly",
                    Password = "onlyread",
                    Database = "workgroup"
                },

                LogWatchFolder = @"c:\ProgramData\Tableau\Tableau Server\data\tabsvc\vizqlserver\Logs\",
                LogWatchMask = "*.txt"

            };
        }

        /// <summary>
        /// Serializes the configuration to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            var outputPath = Path.Combine(config.AgentFolder, ConfigFileName);
            try
            {
                // Collect the process information
                var processes = new List<string>();
                foreach(ListViewItem item in processListView.Items)
                {
                    processes.Add(item.Text);
                }
                // serialize the config
                PalMonConfiguration configOut = ConfigConverter.ConfigToPalMonConfig(Config, processes);
                XmlSerializer writer = new XmlSerializer(configOut.GetType());
                using (var file = new FileStream(outputPath, FileMode.Create))
                {
                    writer.Serialize(file, configOut);
                    MessageBox.Show("Configuration successfuly saved to:\n" + outputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Cannot save configuration to:\n{0}\nreason:\n{1}", outputPath, ex.Message));

            }
        }

        #region processes

        private void addProcessButton_Click(object sender, EventArgs e)
        {
            // Add a new item to the ListView, with an empty label
            ListViewItem item = processListView.Items.Add(String.Empty);

            // Place the newly-added item into edit mode immediately
            item.BeginEdit();
        }

        private void deleteProcessBtn_Click(object sender, EventArgs e)
        {
            var selection = processListView.SelectedItems;
            foreach (ListViewItem s in selection)
            {
                processListView.Items.Remove(s);
            }
        }

        #endregion
    }
}
