using System;
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
            PalMonConfiguration configOut = ConfigConverter.ConfigToPalMonConfig(Config);
            XmlSerializer writer = new XmlSerializer(configOut.GetType());
            var outputPath = Path.Combine(config.AgentFolder, ConfigFileName);
            using (var file = new FileStream(outputPath, FileMode.Create))
            {
                writer.Serialize(file, configOut);
            }
        }
    }
}
