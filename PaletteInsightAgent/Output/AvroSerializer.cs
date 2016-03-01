using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Schema;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PaletteInsightAgent.Output
{
    class AvroSerializer : IWriter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string Extension { get { return ".avro"; } }

        public void WriteDataFile(string fileName, DataTable table)
        {
            string schemaString = GetSchemaJSON(table);
            var serializer = Microsoft.Hadoop.Avro.AvroSerializer.CreateGeneric(schemaString);
            using (var streamWriter = File.AppendText(fileName))
            {
                foreach (DataRow row in table.Rows)
                {
                    dynamic record = new AvroRecord(serializer.WriterSchema);
                    foreach (DataColumn column in table.Columns)
                    {
                        copyValue(record, row, column);
                    }
                    serializer.Serialize(streamWriter.BaseStream, record);
                }
            }
        }

        private void copyValue(dynamic record, DataRow row, DataColumn column)
        {
            if (!row.IsNull(column))
            {
                if (column.DataType == typeof(System.DateTime))
                {
                    record[column.ColumnName] = ((DateTime)row[column.ColumnName]).Ticks;
                }
                else
                {
                    record[column.ColumnName] = row[column.ColumnName];
                }
            }
        }

        private string GetSchemaJSON(DataTable table)
        {
            var schema = Schema.CreateRecord("asd", "namespace");
            List<RecordField> fields = new List<RecordField>();
            foreach (DataColumn column in table.Columns)
            {
                fields.Add(GetField(column.ColumnName, column));
            }
            Schema.SetFields(schema, fields);
            return schema.ToString();
        }

        RecordField GetField(string name, DataColumn column, bool nullable = true)
        {
            TypeSchema avroType = getTypeDescriptor(column);
            if (nullable)
            {
                TypeSchema[] unionType = { Schema.CreateNull(), avroType };
                avroType = Schema.CreateUnion(unionType);
            }
            return Schema.CreateField(name, avroType);
        }

        private TypeSchema getTypeDescriptor(DataColumn column)
        {
            switch (column.DataType.ToString())
            {
                case "System.DateTime": // Logical type needed
                    {
                        var ret = Schema.CreateLong();
                        ret.AddAttribute("logicalType", "timestamp-micros");
                        return ret;
                    }
                case "System.String":
                    return Schema.CreateString();
                case "System.Double":
                    return Schema.CreateDouble();
                case "System.Single":
                    return Schema.CreateFloat();
                case "System.Int64":
                case "System.UInt64":
                    return Schema.CreateLong();
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                    return Schema.CreateInt();
                case "System.Boolean":
                    return Schema.CreateBoolean();
                case "System.Byte":
                case "System.SByte":
                    return Schema.CreateBytes();
                case "System.Decimal":
                    {
                        var ret = Schema.CreateBytes();
                        // var precision = column.DataType.
                        ret.AddAttribute("logicalType", "decimal");
                        // ret.AddAttribute("precision", precision);
                        // ret.AddAttribute("scale", scale);
                        return ret;
                    }
                default:
                    Log.Error("Not handled data type: ", column.DataType.ToString());
                    return Schema.CreateString();
            }
        }


    }
}