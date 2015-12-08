namespace TabMon.LogPoller.Db
{
    public class PostgresDbQueries : IDbQueries
    {
        // Our query goes from the oldest to the newest unknown entries
        public string SELECT_FSA_TO_UPDATE_SQL { get { return @"SELECT id, sess, ts FROM filter_state_audit WHERE workbook= '<WORKBOOK>' AND view='<VIEW>' AND ts < @ts AND ts > @min_ts ORDER BY ts asc LIMIT 100"; } }

        public string UPDATE_FSA_SQL { get { return @"UPDATE filter_state_audit SET workbook=@workbook, view=@view, user_ip=@user_ip WHERE id = @id"; } }

        public string HAS_FSA_TO_UPDATE_SQL { get { return @"SELECT COUNT(1) FROM filter_state_audit WHERE workbook = '<WORKBOOK>' AND view = '<VIEW>' AND ts < @ts AND ts > @min_ts"; } }

    }

}
