using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTableWriter.Connection;

namespace DataTableWriter.Drivers
{
    class MsSQLDriver : IDbDriver
    {
        public string Name
        {
            get
            {
                return "MS SQL Driver";
            }
        }

        // Map of MS SQL types -> System types.
        protected readonly IReadOnlyDictionary<string, string> MsSQLToSystemTypeMap
            = new Dictionary<string, string>()
            {
                { "BIT", "System.Boolean" },
                { "BIGINT", "System.Int64" },
                { "INT", "System.Int32" },
                { "SMALLINT", "System.Int16" },
                { "TINYINT", "System.Byte" },
                { "REAL", "System.Single" },
                { "FLOAT(24)", "System.Single" },
                { "FLOAT(53)", "System.Double" },
                { "FLOAT", "System.Double" },

                { "BINARY(50)", "System.Data.Linq.Binary" },
                { "VARBINARY(50)", "System.Data.Linq.Binary" },
                { "VARBINARY(MAX)", "System.Data.Linq.Binary" },
                { "IMAGE", "System.Data.Linq.Binary" },
                { "TIMESTAMP", "System.Data.Linq.Binary" },

                { "SMALLMONEY", "System.Decimal" },
                { "MONEY", "System.Decimal" },
                { "DECIMAL", "System.Decimal" },
                { "NUMERIC", "System.Decimal" },

                { "CHAR", "System.String" },
                { "NCHAR", "System.String" },
                { "VARCHAR", "System.String" },
                { "NVARCHAR", "System.String" },
                { "TEXT", "System.String" },
                { "NTEXT", "System.String" },
                { "XML", "System.Xml.Linq.XElement" },

                { "SMALLDATETIME", "System.DateTime" },
                { "DATETIME", "System.DateTime" },
                { "DATETIME2", "System.DateTime" },
                { "DATETIMEOFFSET", "System.DateTimeOffset" },
                { "DATE", "System.DateTime" },
                { "TIME", "System.TimeSpan" },

                { "UNIQUEIDENTIFIER", "System.Guid" },
                { "SQL_VARIANT", "System.Object" }
                
            };

        // Map of System types -> MS SQL types.
        protected readonly IReadOnlyDictionary<string, string> systemToMsSQLTypeMap
            = new Dictionary<string, string>()
            {
                { "System.Boolean", "BIT"  },
                { "System.Int64", "BIGINT" },
                { "System.Int32", "INT" },
                { "System.Int16", "SMALLINT" },
                { "System.Byte", "TINYINT"  },
                { "System.Single", "REAL" },
                { "System.Double", "FLOAT"  },
                { "System.Decimal", "DECIMAL(29,4)" },

                { "System.Char", "NCHAR(1)" },
                { "System.String", "NVARCHAR(4000)" },
                { "System.Char[]", "NVARCHAR(4000)" },

                { "System.DateTime", "DATETIME" },
                { "System.DateTimeOffset", "DATETIMEOFFSET" },
                { "System.TimeSpan", "TIME"  },

                { "System.Data.Linq.Binary", "VARBINARY(MAX)"  },
                { "System.Guid", "UNIQUEIDENTIFIER"  },
                { "System.Object", "SQL_VARIANT" }
            };

        /// <summary>
        /// Builds a Postgres connection object for the given remote Postgres server.
        /// </summary>
        /// <param name="connectionInfo">The database connection information.</param>
        /// <returns>Connection object for the remote Postgres server.</returns>
        public IDbConnection BuildConnection(IDbConnectionInfo connectionInfo)
        {
            if (connectionInfo.Port == null)
            {
                throw new ArgumentException("Port cannot be null!");
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = connectionInfo.Server;     // +","+connectionInfo.Port;
            builder.InitialCatalog = connectionInfo.DatabaseName;
            builder.UserID = connectionInfo.Username;
            builder.Password = connectionInfo.Password;
            
            builder.IntegratedSecurity = true;
                                  
            return new SqlConnection(builder.ConnectionString);
        }

        /// <summary>
        /// Builds an MS SQL alter statement to add a column to a table.
        /// </summary>
        /// <param name="tableName">The name of the table to add the column to.</param>
        /// <param name="column">The column to add.</param>
        /// <returns>MS SQL alter statement to add the column.</returns>
        public string BuildQueryAddColumnToTable(string tableName, DataColumn column)
        {
            var defaultClause = "";
            if (!column.AllowDBNull)
            {
                defaultClause = GetDefaultValueClause(column.DefaultValue);
            }

            return String.Format(@"ALTER TABLE ""{0}"" ADD COLUMN {1} {2};", tableName, GetStandardColumnSpecification(column.ColumnName, MapToDbType(column.ColumnName, column.DataType.ToString(), column.AllowDBNull)), defaultClause);

        }

        /// <summary>
        /// Generates the MS SQL dialect expression of a default value.
        /// </summary>
        /// <param name="defaultValue">The default value to assign.</param>
        /// <returns>MS SQL expression of a default value statement.</returns>
        private string GetDefaultValueClause(object defaultValue)
        {
            return String.Format("DEFAULT '{0}'", defaultValue);
        }

        /// <summary>
        /// Builds an MS SQL query to retrieve the names and types of all columns in a table.
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve information about.</param>
        /// <param name="excludeIdentityColumn">Flag indicating whether the statement should exclude the ID column.</param>
        /// <returns>MS SQL query that will retrieve the names and types of all columns in the designated table.</returns>
        public string BuildQueryColumnNamesAndTypes(string tableName, bool excludeIdentityColumn = true)
        {
            //TODO: check this!
            var excludeIdentityColumnClause = excludeIdentityColumn ? "AND upper(COLUMN_NAME) != 'ID'" : "";
            return String.Format(" SELECT COLUMN_NAME AS name, DATA_TYPE AS type, IS_NULLABLE AS nullable FROM INFORMATION_SCHEMA.COLUMNS "
                + "WHERE upper(table_name) = '{0}' {1}", tableName.ToUpper(), excludeIdentityColumnClause);
        }

        /// <summary>
        /// Builds a table creation statement in MS SQL DDL.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <param name="columns">The names of the columns to include in the table.</param>
        /// <returns>MS SQL DDL for creation of a table with the given name & columns.</returns>
        public string BuildQueryCreateTable(string tableName, ICollection<string> columns)
        {
            return String.Format(@"CREATE TABLE ""{0}""({1});", tableName, String.Join(",\n", columns));
        }

        /// <summary>
        /// Builds an MS SQL query to insert a row of data into a table.
        /// </summary>
        /// <param name="tableName">The name of the table to insert data into.</param>
        /// <param name="columnList">The list of columns that make up the row.</param>
        /// <param name="parameterList">A collection of parameters to insert.</param>
        /// <returns>MS SQL insert statement to insert the given data into the designated table.</returns>
        public string BuildQueryInsertRow(string tableName, ICollection<string> columnList, IDataParameterCollection parameterList)
        {
            var columns = String.Join(",", columnList);
            ICollection<string> paramNameList = new List<string>();
            foreach (IDbDataParameter parameter in parameterList)
            {
                paramNameList.Add(parameter.ParameterName);
            }
            var paramNames = String.Join(",", paramNameList);
            return String.Format(@"INSERT INTO ""{0}"" ({1}) VALUES ({2});", tableName, columns, paramNames);
        }

        /// <summary>
        /// Builds an MS SQL table selection statement.
        /// </summary>
        /// <param name="tableName">The name of the table to select.</param>
        /// <returns>MS SQL select statement for the given table.</returns>
        public string BuildQuerySelectTable(string tableName)
        {
            return String.Format("SELECT * FROM information_schema.tables WHERE upper(table_name)='{0}'", tableName.ToUpper());
        }

        /// <summary>
        /// Generates the MS SQL dialect expression of an identity column specification.
        /// </summary>
        /// <returns>MS SQL expression of an identity column specification.</returns>
        public string GetIdentityColumnSpecification()
        {
            return "\"id\" int IDENTITY(1,1) PRIMARY KEY";
        }

        public string GetStandardColumnSpecification(string columnName, string columnType)
        {
            return String.Format("\"{0}\" {1}", columnName, columnType);
        }

        /// <summary>
        /// Maps the name of a C# System type to a MS SQL type.
        /// </summary>
        /// <param name="systemType">The name of the System type.</param>
        /// <param name="allowDbNull">Flag indicating whether the input type is nullable.</param>
        /// <returns>MS SQL type that correlates to the given System type.</returns>
        public string MapToDbType(string columnName, string systemType, bool allowDbNull)
        {
            string MsSQLType;
            if (!systemToMsSQLTypeMap.ContainsKey(systemType))
            {
                MsSQLType = "TEXT";
            }
            else
            {
                MsSQLType = systemToMsSQLTypeMap[systemType];
            }

            if (!allowDbNull)
            {
                MsSQLType = String.Format("{0} NOT NULL", MsSQLType);
            }

            return MsSQLType;
        }

        /// <summary>
        /// Maps the name of an MS SQL type to a C# System type.
        /// </summary>
        /// <param name="dbType">The name of the Postgres type.</param>
        /// <returns>System type that correlates to the given MS SQL type.</returns>
        public Type MapToSystemType(string dbType)
        {
            // We only take the first word of the MS SQL type into consideration, in order to simplify our mapping.
            var MsSQLFirstTermOfType = dbType.Split(' ')[0].ToUpper();
            if (!MsSQLToSystemTypeMap.ContainsKey(MsSQLFirstTermOfType))
            {
                return Type.GetType("System.String");
            }

            return Type.GetType(MsSQLToSystemTypeMap[MsSQLFirstTermOfType]);
        }

        public string QueryParamName(string base_param_name)
        {
            return "@" + base_param_name;
        }
    }
}
