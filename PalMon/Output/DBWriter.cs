using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    // CSV Folder:
    // serverlog-2016-01-28-15-06-00.csv
    // serverlog-2016-01-28-15-06-30.csv
    // threadinfo-2016-01-28-15-06-00.csv


    class DBWriter
    {
        public static void Start()
        {
            foreach (var file in GetFilesOfSameTable())
            {
                // Create BULK COPY command
                // BULK COPY
                // Move files to processed folder
            }
        }

        // Gives back a list of files for the same table empty list otherwise
        public static ICollection<string> GetFilesOfSameTable()
        {
            return new List<string>();
        }
    }
}
