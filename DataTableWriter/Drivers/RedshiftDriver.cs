using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableWriter.Drivers
{
    internal class RedshiftDriver : PostgresDriver
    {
        public override string GetIdentityColumnSpecification()
        {
            return "id bigint identity PRIMARY KEY";
        }
    }
}
