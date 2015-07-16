using Oracle.ManagedDataAccess.Client;

namespace fakebook
{
    public class ConnectTheDB
    {
        OracleConnection conn;
        public OracleConnection getDb()
        {
            string oradb = "Data Source=(DESCRIPTION="
             + "(ADDRESS=(PROTOCOL=TCP)(HOST=221.239.198.40)(PORT=1521))"
             + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));"
             + "User Id=c##happy;Password=123456;";
            conn = new OracleConnection(oradb);
            return conn;
        }      
    }
}