using NLog;
using System.Reflection;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PalMon.LogPoller
{
    public class LogPollerConfigurationSection : ConfigurationSection
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


    public partial class LogPollerConfigElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override

        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        public override bool IsReadOnly()
        {
            return false;
        }

        #endregion IsReadOnly override

        #region Name Property

        /// <summary>
        /// The XML name of the <see cref="Directory"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string DirectoryPropertyName = "Directory";

        /// <summary>
        /// Gets or sets the name of the instantiated database.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("The name of the instantiated database.")]
        [global::System.Configuration.StringValidatorAttribute(InvalidCharacters = "", MaxLength = 2147483647, MinLength = 1)]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.LogPollerConfigElement.DirectoryPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false, DefaultValue = "PalMon")]
        public virtual string Directory
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.LogPollerConfigElement.DirectoryPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.LogPollerConfigElement.DirectoryPropertyName] = value;
            }
        }

        #endregion Name Property

        #region Server Property

        /// <summary>
        /// The XML name of the <see cref="Filter"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string FilterPropertyName = "Filter";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.LogPollerConfigElement.FilterPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Filter
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.LogPollerConfigElement.FilterPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.LogPollerConfigElement.FilterPropertyName] = value;
            }
        }

        #endregion Server Property

        //#region User Property

        ///// <summary>
        ///// The XML name of the <see cref="User"/> property.
        ///// </summary>
        //[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        //internal const string UserPropertyName = "User";

        ///// <summary>
        ///// Gets or sets contains information about the database user.
        ///// </summary>
        //[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        //[global::System.ComponentModel.DescriptionAttribute("Contains information about the database user.")]
        //[global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.LogPollerConfigElement.UserPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        //public virtual global::PalMon.LogPoller.User User
        //{
        //    get
        //    {
        //        return ((global::PalMon.LogPoller.User)(base[global::PalMon.LogPoller.LogPollerConfigElement.UserPropertyName]));
        //    }
        //    set
        //    {
        //        base[global::PalMon.LogPoller.LogPollerConfigElement.UserPropertyName] = value;
        //    }
        //}

        //#endregion User Property

        //#region Table Property

        ///// <summary>
        ///// The XML name of the <see cref="Table"/> property.
        ///// </summary>
        //[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        //internal const string TablePropertyName = "Table";

        ///// <summary>
        ///// Gets or sets contains information about the results table.
        ///// </summary>
        //[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        //[global::System.ComponentModel.DescriptionAttribute("Contains information about the results table.")]
        //[global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.LogPollerConfigElement.TablePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        //public virtual global::PalMon.LogPoller.Table Table
        //{
        //    get
        //    {
        //        return ((global::PalMon.LogPoller.Table)(base[global::PalMon.LogPoller.LogPollerConfigElement.TablePropertyName]));
        //    }
        //    set
        //    {
        //        base[global::PalMon.LogPoller.LogPollerConfigElement.TablePropertyName] = value;
        //    }
        //}

        //#endregion Table Property
    }

    class LogPollerConfigurationLoader
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
                Log.Fatal(ex, "Could not open configuration file: {0}", ex);
                throw;
            }


            // Get the section
            try
            {
                return config.GetSection("LogPoller") as LogPollerConfigurationSection;
            }
            catch (ConfigurationErrorsException err)
            {
                Log.Fatal(err, "Cannot find configuration section: 'LogPoller': {0}", err.ToString());
                throw;
            }

        }
    }
}
