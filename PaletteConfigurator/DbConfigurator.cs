using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PaletteConfigurator.DbTesters;

namespace PaletteConfigurator
{
    public partial class DbConfigurator : UserControl
    {

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DbDetails Database
        {
            get { return database; }
            set
            {
                database = value;
                dbDetailsBindingSource.DataSource = database;
            }
        }

        private DbDetails database;

        public DbConfigurator()
        {
            InitializeComponent();

            SetupDataBindings();
            SetupDbTypeSelector();

            Database = new DbDetails
            {
                DbType = "Postgres",
                Host = "localhost",
                Port = 5432,
                Username = "postgres",
                Password = "abc12def",
                Database = "TabMon"
            };

        }

        private void SetupDbTypeSelector()
        {
            var dbTypes = "Postgres".Split(new char[] { ',' })
                .Select((t) => DbTesterFactory.CreateDbTester(t))
                .ToList();

            databaseTypeCombobox.DataSource = dbTypes;
            databaseTypeCombobox.DisplayMember = "Name";
            // TODO: this is sooo ugly: double create IDbTester instances on testing
            databaseTypeCombobox.ValueMember = "Name";
        }

        private void SetupDataBindings()
        {
            hostNameText.DataBindings.Add(new Binding("Text", dbDetailsBindingSource, "Host", true, DataSourceUpdateMode.OnPropertyChanged));
            usernameText.DataBindings.Add(new Binding("Text", dbDetailsBindingSource, "Username", true, DataSourceUpdateMode.OnPropertyChanged));
            passwordText.DataBindings.Add(new Binding("Text", dbDetailsBindingSource, "Password", true, DataSourceUpdateMode.OnPropertyChanged));
            databaseText.DataBindings.Add(new Binding("Text", dbDetailsBindingSource, "Database", true, DataSourceUpdateMode.OnPropertyChanged));
            portSpinner.DataBindings.Add(new Binding("Value", dbDetailsBindingSource, "Port", true, DataSourceUpdateMode.OnPropertyChanged));
        }

        /// <summary>
        /// shows the progress bar as a hint to working
        /// </summary>
        /// <param name="act"></param>
        private void withProgressBar(Action act)
        {
            // show the progress bar as a hint to working
            progressBar1.Show();
            testConnectionButton.Hide();
            try
            {
                act();
            }
            finally
            {
                progressBar1.Hide();
                testConnectionButton.Show();
            }
        }


        private void testConnectionButton_Click(object sender, EventArgs e)
        {
            withProgressBar(() =>
            {
                var tester = DbTesters.DbTesterFactory.CreateDbTester((string)databaseTypeCombobox.SelectedValue);
                var testResult = tester.VerifyConnection(database);

                // show the success
                if (testResult.success)
                {
                    MessageBox.Show("Connection test successful");
                    progressBar1.Visible = false;
                    return;
                }

                // show the failiure message
                MessageBox.Show(
                    String.Format("{1}\nWhile connecting to:{0}",
                        tester.ConnectionString(database), testResult.message));
            });
        }
    }
}
