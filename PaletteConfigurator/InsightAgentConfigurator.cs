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

        private const string ConfigFolderName = "Config";
        private const string ConfigFileName = "PalMon.Config";
        private InsightAgentConfiguration config;

        public InsightAgentConfigurator()
        {
            InitializeComponent();

            InitializeConfiguration();
            SetupBindings();


            clusterTreeView.BeginUpdate();
            clusterTreeView.Nodes.Clear();
            string yourParentNode;
            yourParentNode = "Primary"; // textBox1.Text.Trim();
            clusterTreeView.Nodes.Add(yourParentNode);
            clusterTreeView.Nodes[0].Nodes.Add("localhost");

            clusterTreeView.ExpandAll();
            clusterTreeView.EndUpdate();
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

        private void LoadFromFolder(string folderName)
        {
            var configPath = GetConfigFilePath(folderName);
            try
            {
                // serialize the config
                XmlSerializer writer = new XmlSerializer(typeof(PalMonConfiguration));
                // try to create the directory
                using (var file = new FileStream(configPath, FileMode.Open))
                {
                    var result = (PalMonConfiguration)writer.Deserialize(file);
                    Config = ConfigConverter.FromPalMonConfig(folderName, result);

                    // Add the processes
                    processListView.BeginUpdate();
                    processListView.Items.Clear();
                    foreach(var process in Config.Processes)
                    {
                        processListView.Items.Add(process);
                    }
                    processListView.EndUpdate();

                    // Add the clusters

                    clusterTreeView.BeginUpdate();
                    clusterTreeView.Nodes.Clear();
                    foreach(var cluster in Config.Clusters)
                    {
                        var clusterNode = clusterTreeView.Nodes.Add(cluster.ClusterName);
                        foreach(var node in cluster.Nodes)
                        {
                            clusterNode.Nodes.Add(node);
                        }
                    }
                    clusterTreeView.ExpandAll();
                    clusterTreeView.EndUpdate();


                    MessageBox.Show("Configuration successfuly loaded from:\n" + configPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Cannot load configuration from:\n{0}\nreason:\n{1}", configPath, ex.Message));

            }

        }

        /// <summary>
        /// Serializes the configuration to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            string outputPath = GetConfigFilePath(config.AgentFolder);
            try
            {
                UpdateProcessesAndClustersInConfig();
                // serialize the config
                PalMonConfiguration configOut = ConfigConverter.ConfigToPalMonConfig(Config);
                XmlSerializer writer = new XmlSerializer(configOut.GetType());
                // try to create the directory
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                }
                catch (Exception)
                {
                    // the directory 
                }

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

        private string GetConfigFilePath(string agentFolder)
        {
            return Path.Combine(agentFolder, ConfigFolderName, ConfigFileName);
        }

        private void UpdateProcessesAndClustersInConfig()
        {
            // Collect the process information
            var processes = new List<string>();
            foreach (ListViewItem item in processListView.Items)
            {
                processes.Add(item.Text);
            }

            // collect the cluster information
            var clusters = new List<ClusterData>();
            foreach (TreeNode cluster in clusterTreeView.Nodes)
            {
                // collect the child nodes
                var clusterNodes = new List<string>();
                foreach (TreeNode clusterNode in cluster.Nodes)
                {
                    clusterNodes.Add(clusterNode.Text);

                }
                clusters.Add(new ClusterData
                {
                    ClusterName = cluster.Text,
                    Nodes = clusterNodes
                });
            }
            // update the configuration
            Config.Processes = processes;
            Config.Clusters = clusters;
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

        private void addClusterButton_Click(object sender, EventArgs e)
        {
            var clusterName = "CLUSTER NAME";
            clusterTreeView.BeginUpdate();
            var newNode = clusterTreeView.Nodes.Add(clusterName);
            clusterTreeView.SelectedNode = newNode;
            clusterTreeView.EndUpdate();
            newNode.BeginEdit();


            
        }

        private void addClusterNodeButton_Click(object sender, EventArgs e)
        {
            var nodeName = "NODE NAME";
            clusterTreeView.BeginUpdate();


            var selected = clusterTreeView.SelectedNode;
            var targetCluster = (selected.Parent == null) ? selected : selected.Parent;
            var newNode = targetCluster.Nodes.Add(nodeName);

            clusterTreeView.SelectedNode = newNode;
            clusterTreeView.ExpandAll();
            clusterTreeView.EndUpdate();

            newNode.BeginEdit();

        }

        private void deleteClusterButton_Click(object sender, EventArgs e)
        {
            clusterTreeView.BeginUpdate();
            var selected = clusterTreeView.SelectedNode;
            var removeFrom = (selected.Parent == null) ? clusterTreeView.Nodes : selected.Parent.Nodes;
            removeFrom.Remove(selected);
            clusterTreeView.EndUpdate();
        }

        private void loadConfigurationButton_Click(object sender, EventArgs e)
        {
            LoadFromFolder(config.AgentFolder);
        }
    }
}
