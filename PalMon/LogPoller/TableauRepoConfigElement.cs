using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.LogPoller
{

    public partial class TableauRepoConfigElement : global::System.Configuration.ConfigurationElement
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

        #region Host Property

        /// <summary>
        /// The XML name of the <see cref="Host"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string HostPropertyName = "Host";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.TableauRepoConfigElement.HostPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Host
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.TableauRepoConfigElement.HostPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.TableauRepoConfigElement.HostPropertyName] = value;
            }
        }

        #endregion Host Property


        #region Port Property

        /// <summary>
        /// The XML name of the <see cref="Port"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string PortPropertyName = "Port";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.TableauRepoConfigElement.PortPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Port
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.TableauRepoConfigElement.PortPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.TableauRepoConfigElement.PortPropertyName] = value;
            }
        }

        #endregion Port Property


        #region Username Property

        /// <summary>
        /// The XML name of the <see cref="Username"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string UsernamePropertyName = "Username";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.TableauRepoConfigElement.UsernamePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Username
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.TableauRepoConfigElement.UsernamePropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.TableauRepoConfigElement.UsernamePropertyName] = value;
            }
        }

        #endregion Username Property


        #region Password Property

        /// <summary>
        /// The XML name of the <see cref="Password"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string PasswordPropertyName = "Password";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.TableauRepoConfigElement.PasswordPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Password
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.TableauRepoConfigElement.PasswordPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.TableauRepoConfigElement.PasswordPropertyName] = value;
            }
        }

        #endregion Password Property


        #region Db Property

        /// <summary>
        /// The XML name of the <see cref="Db"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string DbPropertyName = "Db";

        /// <summary>
        /// Gets or sets contains information about the database server location.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [global::System.ComponentModel.DescriptionAttribute("Contains information about the database server location.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::PalMon.LogPoller.TableauRepoConfigElement.DbPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public virtual string Db
        {
            get
            {
                return ((string)(base[global::PalMon.LogPoller.TableauRepoConfigElement.DbPropertyName]));
            }
            set
            {
                base[global::PalMon.LogPoller.TableauRepoConfigElement.DbPropertyName] = value;
            }
        }

        #endregion Db Property

    }

}
