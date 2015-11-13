using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTableWriter.Connection;
using Oracle.ManagedDataAccess.Client;

using log4net;
using System.Reflection;

namespace DataTableWriter.Drivers
{
    class OracleDriver : IDbDriver
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public string QueryParamName(string base_param_name) { return ":" + base_param_name;  }
        public string Name
        {
            get
            {
                return "Oracle Driver";
            }
        }

        // Map of Postgres types -> System types.
        protected readonly IReadOnlyDictionary<string, string> postgresToSystemTypeMap
            = new Dictionary<string, string>()
            {
                { "BIGINT", "System.Int64" },
                { "INTEGER", "System.Int32" },
                { "SMALLINT", "System.Int16" },
                { "BINARY_DOUBLE", "System.Double" },
                { "BINARY_FLOAT", "System.Single" },

                //{ "bigserial", "System.Int64" },
                //{ "boolean", "System.Boolean" },
                { "CHAR", "System.Boolean" },


                //{ "NUMBER", "System.Decimal" },
                { "NUMBER", "System.Int32" },
                //{ "smallserial", "System.Int16" },
                //{ "text", "System.String" },
                { "CLOB", "System.String" },
                { "TIMESTAMP", "System.DateTime" },
                { "TIMESTAMP(9)", "System.DateTime" },
                { "TIMESTAMP(6)", "System.DateTime" }
            };

        // Map of System types -> Postgres types.
        protected readonly IReadOnlyDictionary<string, string> systemToPostgresTypeMap
            = new Dictionary<string, string>()
            {
                { "System.Boolean", "char" },
                { "System.Byte", "smallint" },
                { "System.Char", "char(1)" },
                { "System.DateTime", "timestamp" },
                //{ "System.Decimal", "number(22,7)" },
                { "System.Decimal", "integer" },
                { "System.Double", "binary_double" },
                { "System.Int16", "smallint" },
                { "System.Int32", "integer" },
                { "System.Int64", "bigint" },
                { "System.Single", "binary_float" },
                { "System.String", "varchar2(255)" },
            };

        public IDbConnection BuildConnection(IDbConnectionInfo connectionInfo)
        {

            if (connectionInfo.Port == null)
            {
                throw new ArgumentException("Port cannot be null!");
            }

            var connectionString = String.Format("Data Source=(DESCRIPTION="
                + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))"
                + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={4})));"
                + "User Id={2};Password={3};",
                    connectionInfo.Server,
                    connectionInfo.Port.Value,
                    connectionInfo.Username,
                    connectionInfo.Password,
                    connectionInfo.DatabaseName
                );

            Log.Info("Connecting to oracle: " + connectionInfo.Server + " service: " + connectionInfo.DatabaseName);
            

            return new OracleConnection(connectionString);
        }



        /// <summary>
        /// Generates the Oracle dialect expression of a default value.
        /// </summary>
        /// <param name="defaultValue">The default value to assign.</param>
        /// <returns>Oracle expression of a default value statement.</returns>
        public string GetDefaultValueClause(object defaultValue)
        {
            return String.Format("DEFAULT '{0}'", defaultValue);
        }

        /// <summary>
        /// Generates the Oracle dialect expression of an identity column specification.
        /// </summary>
        /// <returns>Oracle expression of an identity column specification.</returns>
        public string GetIdentityColumnSpecification()
        {
            return "\"id\" integer";
        }


        /// <summary>
        /// Returns the sql statement a sequence for the 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string generatePKSequence(string tableName)
        {
            return String.Format(
                "\nBEGIN " 
                + "  EXECUTE IMMEDIATE 'DROP SEQUENCE {0}_seq';"
                + "  EXCEPTION WHEN OTHERS THEN NULL;"
                + "END;\n/\n"
                + "CREATE SEQUENCE {0}_seq START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE; ", tableName);
        }


        /// <summary>
        /// Generates the oracle ALTER TABLE statement for a primary key column.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string generatePkAlterTable(string tableName)
        {
            return String.Format("ALTER TABLE {0} ADD (CONSTRAINT {0}_pk PRIMARY KEY(\"id\")); ", tableName);
        }


        /// <summary>
        ///  Generates the Primary key trigger for oracle
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns></returns>
        private string generatePkTrigger(string tableName)
        {
            return String.Format("CREATE OR REPLACE TRIGGER {0}_id_trig "
                + "BEFORE INSERT ON {0} "
                + "FOR EACH ROW "
                + "BEGIN "
                + "SELECT {0}_seq.NEXTVAL "
                + "INTO :new.\"id\" "
                + "FROM   dual; "
                + "END; ", tableName);
        }

        /// <summary>
        /// Generates the Oracle dialect expression of a standard column specification.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnType">The data type of the column.</param>
        /// <returns>Oracle expression of a column with the given name & type.</returns>
        public string GetStandardColumnSpecification(string columnName, string columnType)
        {
            return String.Format("\"{0}\" {1}", columnName, columnType);
        }

        /// <summary>
        /// Builds a table creation statement in Oracle DDL.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <param name="columns">The names of the columns to include in the table.</param>
        /// <returns>Oracle DDL for creation of a table with the given name & columns.</returns>
        public string BuildQueryCreateTable(string tableName, ICollection<string> columns)
        {
            //var triggerDef = generatePkTrigger(tableName);
            //var sequenceDef = generatePKSequence(tableName);
            //var pkAlterTableDef = generatePkAlterTable(tableName);
            var tableCreateDef = String.Format(@"CREATE TABLE {0} ({1})", tableName, String.Join(",\n", columns));
            //var createStatement = tableCreateDef + pkAlterTableDef + sequenceDef + triggerDef;
            return tableCreateDef;
        }

        /// <summary>
        /// Builds a Oracle table selection statement.
        /// </summary>
        /// <param name="tableName">The name of the table to select.</param>
        /// <returns>Oracle select statement for the given table.</returns>
        public string BuildQuerySelectTable(string tableName)
        {
            return String.Format("SELECT * FROM ALL_OBJECTS WHERE upper(OBJECT_NAME) = '{0}'", tableName.ToUpper());
        }


        /// <summary>
        /// Builds a Postgres alter statement to add a column to a table.
        /// </summary>
        /// <param name="tableName">The name of the table to add the column to.</param>
        /// <param name="column">The column to add.</param>
        /// <returns>Postgres alter statement to add the column.</returns>
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
        /// Builds a Postgres query to retrieve the names and types of all columns in a table.
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve information about.</param>
        /// <param name="excludeIdentityColumn">Flag indicating whether the statement should exclude the ID column.</param>
        /// <returns>Postgres query that will retrieve the names and types of all columns in the designated table.</returns>
        public string BuildQueryColumnNamesAndTypes(string tableName, bool excludeIdentityColumn = true)
        {
            var excludeIdentityColumnClause = excludeIdentityColumn ? "AND upper(COLUMN_NAME) != 'ID'" : "";
            return String.Format(" SELECT COLUMN_NAME AS name, DATA_TYPE AS type, NULLABLE AS nullable FROM ALL_TAB_COLUMNS "
                + "WHERE upper(table_name) = '{0}' {1}", tableName.ToUpper(), excludeIdentityColumnClause);

            //var excludeIdentityColumnClause = excludeIdentityColumn ? "and a.attname != 'id'" : "";
            //return String.Format(@"SELECT a.attname AS name, format_type(a.atttypid, a.atttypmod) AS type, NOT(a.attnotnull) AS nullable
            //                       FROM pg_attribute a JOIN pg_class b ON (a.attrelid = b.relfilenode)
            //                       WHERE b.relname = '{0}' AND a.attstattarget = -1 {1};", tableName, excludeIdentityColumnClause);
        }


        /// <summary>
        /// Builds a Postgres query to insert a row of data into a table.
        /// </summary>
        /// <param name="tableName">The name of the table to insert data into.</param>
        /// <param name="columnList">The list of columns that make up the row.</param>
        /// <param name="parameterList">A collection of parameters to insert.</param>
        /// <returns>Postgres insert statement to insert the given data into the designated table.</returns>
        public string BuildQueryInsertRow(string tableName, ICollection<string> columnList, IDataParameterCollection parameterList)
        {
            var columns = String.Join(",", columnList);
            ICollection<string> paramNameList = new List<string>();
            foreach (IDbDataParameter parameter in parameterList)
            {
                paramNameList.Add(parameter.ParameterName);
            }
            var paramNames = String.Join(",", paramNameList);
            var insertExpr = String.Format(@"INSERT INTO ""{0}"" ({1}) VALUES ({2})", tableName.ToUpper(), columns, paramNames);

            return insertExpr;
        }

        /// <summary>
        /// Maps the name of a Postgres type to a C# System type.
        /// </summary>
        /// <param name="pgType">The name of the Postgres type.</param>
        /// <returns>System type that correlates to the given Postgres type.</returns>
        public Type MapToSystemType(string pgType)
        {
            // We only take the first word of the postgres type into consideration, in order to simplify our mapping.
            var pgFirstTermOfType = pgType.Split(' ')[0].ToUpper();
            if (!postgresToSystemTypeMap.ContainsKey(pgFirstTermOfType))
            {
                return Type.GetType("System.String");
            }


            return Type.GetType(postgresToSystemTypeMap[pgFirstTermOfType]);
        }


        /// <summary>
        /// The list of clob fields
        /// </summary>
        private string[] clobFields = new string[] { "v" };

        /// <summary>
        /// Maps the name of a C# System type to a Postgres type.
        /// </summary>
        /// <param name="systemType">The name of the System type.</param>
        /// <param name="allowDbNull">Flag indicating whether the input type is nullable.</param>
        /// <returns>Postgres type that correlates to the given System type.</returns>
        public string MapToDbType(string columnName, string systemType, bool allowDbNull)
        {
            string pgType;
            if (!systemToPostgresTypeMap.ContainsKey(systemType))
            {
                pgType = "varchar2(255)";
            }
            else
            {
                pgType = systemToPostgresTypeMap[systemType];
            }

            // Check if this field needs to be a clob
            if (clobFields.Contains(columnName))
            {
                pgType = "clob";
            }

            if (!allowDbNull)
            {
                pgType = String.Format("{0} NOT NULL", pgType);
            }

            return pgType;
        }

    }
}
