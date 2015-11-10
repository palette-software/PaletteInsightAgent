using log4net;
using System.Reflection;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TabMon.LogPoller
{
    class LogPollerConfigurationSection : ConfigurationSection
    {

        public LogPollerConfigurationSection()
        {

        }


        [ConfigurationProperty("directory", DefaultValue = @"C:\Tableau\Logs", IsRequired = true, IsKey = true)]
        public string Directory
        {
            get { return (string)this["directory"]; }
            set { this["directory"] = value; }
        }

        [ConfigurationProperty("filter", DefaultValue = "*.*", IsRequired = false)]
        public string Filter
        {
            get { return (string)this["filter"]; }
            set { this["filter"] = value; }
        }



        //[ConfigurationProperty("interval", DefaultValue = (int)5000, IsRequired = false)]
        //[IntegerValidator(MinValue = 10, MaxValue = 100000, ExcludeRange = false)]
        //public int Port
        //{
        //    get
        //    {
        //        return (int)this["interval"];
        //    }
        //    set
        //    {
        //        this["interval"] = value;
        //    }
        //}
    }


    class LogPollerConfigurationLoader
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static LogPollerConfigurationSection load()
        {
            Configuration config;
            // Load the config file
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Fatal(String.Format("Could not open configuration file: {0}", ex.Message));
                throw;
            }


            // Get the section
            try
            {
                return config.GetSection("LogPoller") as LogPollerConfigurationSection;
            }
            catch (ConfigurationErrorsException err)
            {
                Log.Fatal(String.Format("Cannot find configuration section: 'LogPoller': {0}", err.ToString()));
                throw;
            }

        }
    }
}
