using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Helpers
{
    class TableHelper
    {
        public static DataColumn addColumn(DataTable table, string colName, string dataType="System.String", bool unique = false, bool readOnly = false)
        {
            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            DataColumn column = new DataColumn();
            column.DataType = System.Type.GetType(dataType);
            column.ColumnName = colName;
            column.ReadOnly = readOnly;
            column.Unique = unique;
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);
            return column;
        }
    };
}
