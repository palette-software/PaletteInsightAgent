using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PaletteInsightAgent.Output
{
    public struct Schema
    {
        public struct Column
        {
            [YamlMember(Alias = "Name")]
            public string Name { get; set; }

            [YamlMember(Alias = "Type")]
            public string Type { get; set; }

            [YamlMember(Alias = "AllowNull")]
            public bool AllowNull { get; set; }

            [YamlMember(Alias = "MaxLength")]
            public int MaxLength { get; set; }

            [YamlMember(Alias = "Ordinal")]
            public int Ordinal { get; set; }
        }

        [YamlMember(Alias = "Columns")]
        public ICollection<Column> Columns { get; set; }
    }

    class SchemaStore
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static IDictionary<string, string> tableSchemas = new Dictionary<string, string>();
        private static readonly IReadOnlyDictionary<string, string> systemToPostgresTypeMap
                    = new Dictionary<string, string>()
                    {
                            { "System.Boolean", "boolean" },
                            { "System.Byte", "smallint" },
                            { "System.Char", "character(1)" },
                            { "System.DateTime", "timestamp" },
                            { "System.DateTimeOffset", "timestamp" },
                            { "System.Decimal", "numeric" },
                            { "System.Double", "double precision" },
                            { "System.Int16", "smallint" },
                            { "System.Int32", "integer" },
                            { "System.Int64", "bigint" },
                            { "System.Single", "float8" },
                            { "System.String", "text" },
                    };

        public static void InitSchema(DataTable table)
        {
            if (!tableSchemas.ContainsKey(table.TableName))
            {
                using (var writer = new StringWriter())
                {
                    var schema = GetSchemaFromTable(table);
                    var yamlSerializer = new Serializer(namingConvention: new NullNamingConvention());
                    yamlSerializer.Serialize(writer, schema);
                    tableSchemas.Add(table.TableName, writer.ToString());
                    Log.Info("Added schema definition for table: {0}", table.TableName);
                }
            }
        }

        public static Schema GetSchemaFromTable(DataTable table)
        {
            var ret = new Schema();
            ret.Columns = new List<Schema.Column>();
            foreach (DataColumn dataColumn in table.Columns)
            {
                var column = new Schema.Column();
                column.Name = dataColumn.ColumnName;
                column.Type = systemToPostgresTypeMap[dataColumn.DataType.ToString()];
                column.AllowNull = dataColumn.AllowDBNull;
                column.Ordinal = dataColumn.Ordinal;
                column.MaxLength = dataColumn.MaxLength;
                ret.Columns.Add(column);
            }
            return ret;
        }
    }
}
