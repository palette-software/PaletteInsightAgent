using DataTableWriter.Helpers;
using System.Data;
using System.Linq;

namespace DataTableWriter
{
    /// <summary>
    /// Extension methods for the System.Data.DataTable class.
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Indicates whether two DataTables have equivalent schema.
        /// </summary>
        /// <param name="dt">This DataTable.</param>
        /// <param name="value">The DataTable to compare this DataTable to.</param>
        /// <returns>True if these DataTables have equivalent schema.</returns>
        public static bool SchemaEquals(this DataTable dt, DataTable value)
        {
            if (dt.Columns.Count != value.Columns.Count)
            {
                return false;
            }

            var dtColumns = dt.Columns.Cast<DataColumn>();
            var valueColumns = value.Columns.Cast<DataColumn>();

            var exceptCount = dtColumns.Except(valueColumns, DataColumnEqualityComparer.instance).Count();
            return (exceptCount == 0);
        }

        /// <summary>
        /// Add a row to the table, where the "id" field is calculated for
        /// the current runcycle, and gets incremented with every insert.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="row"></param>
        /// <param name="baseId"></param>
        public static void AddRowWithBaseId(this DataTable dt, DataRow row, ref long baseId)
        {
            row["id"] = baseId++;
            dt.Rows.Add(row);
        }
    }
}
