using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace fakebook.Controllers
{
    public class WebViewController : Controller
    {
        // GET: WebView
        public ActionResult Fakebook()
        {


            Console.WriteLine("fakebook");
            
            

            string oradb = "Data Source=(DESCRIPTION="
             + "(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))"
             + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));"
             + "User Id=monzy;Password=123456;";
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string selectStr = "select * from test1";
                string insertStr = "insert into test(t) values(500)";
                OracleCommand cmd = new OracleCommand(insertStr, conn);
                //OracleDataReader dr = cmd.ExecuteReader();
                //dr.Read();
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                //return View("insertedLines: " + insertedLines);
            }
            catch (OracleException ex) // catches only Oracle errors
            {
                switch (ex.Number)
                {
                    case 1:
                        //return "Error attempting to insert duplicate data.";
                    case 12545:
                        //return "The database is unavailable.";
                    default:
                        return View();
                        //return "Database error: " + ex.Message.ToString();
                }
            }
            catch (Exception ex) // catches any error not previously caught
            {
                Console.Write("Other exception");
            }
            Console.WriteLine("success");

            return View();
        }
    }
}