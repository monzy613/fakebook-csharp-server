using fakebook.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fakebook.Controllers
{
    public class LeaveMessageController : Controller
    {
        static int pagecount = 10;
        // GET: LeaveMessage
        public ActionResult Index(string change)
        {
            //session for current friend
            string username = "h2";
            touser = username;
            Debug.WriteLine("change to " + change);
            int page = 0;
            if (change == null)
            {
                page = 1;
            }
            else
            {
                page = Int32.Parse(change);
            }
            OracleConnection conn = new ConnectTheDB().getDb();
            try
            {
                conn.Open();
                string selectStr = "select fromuser,message,message_date,headimg from leavemessage,fake_user where username=fromuser and touser='"+username+"' order by message_date desc ";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                Debug.WriteLine("获取数据成功！");
                List<LeaveM> Messages = new List<LeaveM>();
                int count = 0;//用于记录当前的条目
                while (dr.Read())//获得数据库的topic然后传回给view
                {
                    if (count >= pagecount * (page - 1) && count < pagecount * page)//用于分页，每页十个条目
                    {
                        LeaveM Message = new LeaveM();
                        Message.fromU = dr.GetString(0);
                        Message.message = dr.GetString(1);
                        Message.showdate = getrealtime(dr.GetInt64(2));
                        Message.userimg = dr.GetString(3);
                        Messages.Add(Message);
                        Debug.WriteLine("image is "+Message.userimg);
                    }
                    count++;
                }
                int allpages = 0;
                if (count == 0)
                {
                    allpages = 1;
                }
                else if (count % pagecount == 0)
                {
                    allpages = count / pagecount;
                }
                else
                {
                    allpages = count / pagecount + 1;
                }
                ViewData["Messages"] = Messages;
                ViewData["current"] = page;
                ViewData["pages"] = allpages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取出错！");
            }
            finally
            {
                conn.Close();
            }
            return View();
        }

        //把long转换成为正常的string
        private string getrealtime(long t)
        {
            string time = t + "";
            //Debug.WriteLine("time is "+time);
            string year = time.Substring(0, 4);
            //Debug.WriteLine("year is " + year);
            string month = time.Substring(4, 2);
            //Debug.WriteLine("month is " + month);
            string day = time.Substring(6, 2);
            //Debug.WriteLine("day is " + day);
            string hour = time.Substring(8, 2);
            //Debug.WriteLine("hour is " + hour);
            string minute = time.Substring(10, 2);
            //Debug.WriteLine("minute is " + minute);
            string realtime = year + "-" + month + "-" + day + " " + hour + ":" + minute;
            return realtime;
        }

        //好友的标记通过session来获得，默认是自己
        static string touser = "happy2";

        //发表新的评论同时也要更新话题的最新时间
        public ActionResult SaveMessage(string comment)
        {
            Debug.WriteLine("new commend is " + comment);
            //usrname session获得
            string username = Session["username"] as string;
            OracleConnection conn = new ConnectTheDB().getDb();
            try
            {
                conn.Open();
                string day = System.DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "");
                string hour = System.DateTime.Now.ToString("HH:mm:ss").Replace(":", "");
                long time = Int64.Parse(day + hour);                     //获得当前的时间
                //string insertStr = "insert into test(id) values(500)";
                string selectStr = "insert into leavemessage (touser,fromuser,message,message_date) values('" + touser + "','"
                    + username + "','" + comment + "'," + time + ")";
                OracleCommand c = new OracleCommand(selectStr, conn);
                int insertedLines = c.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取出错！");
            }
            finally
            {
                conn.Close();
            }
            Response.Redirect("/LeaveMessage/Index");
            return View();
        }
    }
}