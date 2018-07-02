using System.Data;

namespace PaletteInsightAgent.Output
{
    interface IWriter
    {
        string Extension { get; }
        void WriteDataFile(string fileName, DataTable table, bool isFullTable, bool writeHeader, string originalFileName="");
    }
}
