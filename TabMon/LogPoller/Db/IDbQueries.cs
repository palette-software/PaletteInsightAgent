using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller.Db
{
    /// <summary>
    /// Wrapper interface for SQL queries related to a DB.
    /// 
    /// The queries themselves are fields so type checks can validate them.
    /// </summary>
    public interface IDbQueries
    {
        string SELECT_FSA_TO_UPDATE_SQL { get; }
        string UPDATE_FSA_SQL { get; }
        string HAS_FSA_TO_UPDATE_SQL { get; }
    }
}
