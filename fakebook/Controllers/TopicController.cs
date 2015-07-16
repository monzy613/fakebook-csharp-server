using fakebook.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;

namespace fakebook.Controllers
{
    public class TopicController : Controller
    {
        int pagecount = 10;
        static string topicid="0";
        // GET: Topic列表
        public ActionResult Index(string change)
        {
            Debug.WriteLine("change to "+change);
            int page=0;
            if (change == null)
            {
                page = 1;
            }else
            {
                  page = Int32.Parse(change);
            }
            OracleConnection conn = new ConnectTheDB().getDb();
            try
            {
                conn.Open();
                string selectStr = "select * from Topic order by topic_date desc ";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                Debug.WriteLine("获取数据成功！");
                List<Topic> topics = new List<Topic>();
                int count=0;//用于记录当前的条目
                while (dr.Read())//获得数据库的topic然后传回给view
                {
                    if (count >= pagecount * (page - 1)&& count < pagecount * page)//用于分页，每页十个条目
                    {
                        Topic topic = new Topic();
                        topic.Topic_id = dr.GetString(0);
                        topic.Topic_name = dr.GetString(5);
                        topic.Topic_Content = dr.GetString(2);
                        topic.Topic_view = dr.GetInt32(4);
                        topic.username = dr.GetString(1);
                        topic.showdate = getrealtime(dr.GetInt64(3));
                        topics.Add(topic);
                       // Debug.WriteLine(dr.GetString(0) + "," + dr.GetString(1) + "," + dr.GetString(2));
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
                ViewData["topics"] = topics;
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
        //新建一个话题
        public ActionResult newtopic()
        {
            return View();
        }

        public ActionResult test()
        {
            return View();
        }


        public ActionResult toptest()
        {
            return View();
        }
        //进入一个话题
        public ActionResult selecttopic(string id,string change)
        {
            Debug.WriteLine("change to " + change);
            OracleConnection conn = new ConnectTheDB().getDb();
            conn.Open();

            int page = 0;
            if (change == null)
            {
                page = 1;
            }
            else
            {
                page = Int32.Parse(change);
            }
            //如果是id=null的话，那么就属于发布评论，view数不加1
            if (id == null)
            {
                id = topicid;
            }
            else//属于新的view,要更新viewnum
            {
                string updateStr = "update topic set viewnum=viewnum+1 where topicid="+id+"";
                OracleCommand c = new OracleCommand(updateStr, conn);
                int insertedLines = c.ExecuteNonQuery();
                topicid = id;
            }

            try
            {
                //获得当前的话题
                string selectStr = "select * from Topic where topicid= '"+topicid+"'";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                Debug.WriteLine("获取数据成功！");
                while (dr.Read())//获得数据库的topic然后传回给view
                {
                    ViewData["topicname"] = dr.GetString(1);
                    ViewData["topiccontent"] = dr.GetString(2);
                    ViewData["topicdate"] = getrealtime(dr.GetInt64(3));
                }
                //获得当前的话题评论
                selectStr = "select TopicComment.content,topiccomment_date,TopicComment.username,headimg from TopicComment,fake_user where topicid='" + topicid + "'and fake_user.username=topiccomment.username order by topiccomment_date desc";
                OracleCommand oc = new OracleCommand(selectStr, conn);
                dr = oc.ExecuteReader();
                Debug.WriteLine("获取数据成功！");
                List<TopicComment> tcs = new List<TopicComment>();
                int count = 0;//用于记录当前的条目
                while (dr.Read())//获得数据库的topic然后传回给view
                {
                    if (count >= pagecount * (page - 1) && count < pagecount * page)//用于分页，每页十个条目
                    {
                        TopicComment tc = new TopicComment();
                        tc.content = dr.GetString(0);
                        //转化成为正常的时间格式
                        long t= dr.GetInt64(1);
                        tc.showdate = getrealtime(t);
                        tc.username = dr.GetString(2);
                        tc.userimg = dr.GetString(3);
                        tcs.Add(tc);
                        //Debug.WriteLine(dr.GetString(2) + "," + dr.GetInt64(3) + "," + dr.GetString(4));
                    }
                    count++;

                }
                ViewData["topicscommend"] = tcs;
                int allpages = 0;
                if (count==0)
                {
                    allpages = 1;
                }
                else if (count % pagecount == 0)
                {
                    allpages = count / pagecount;
                }else
                {
                    allpages = count / pagecount + 1;
                }
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
        [HttpPost]
        //向数据库里面加入新的话题
        public ActionResult SaveTopic(String newtopic,String newtopicname)
        {

            Debug.WriteLine("new topic is "+ newtopic);
            string username = Session["username"] as string;
            OracleConnection conn = new ConnectTheDB().getDb();
            try
            {
                conn.Open();
                string selectStr = "select count (topicid) from topic";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                //Debug.WriteLine("获取数据成功！");
                dr.Read();
                int num = dr.GetInt32(0);
                string day= System.DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "");
                string hour = System.DateTime.Now.ToString("HH:mm:ss").Replace(":", "");
                long  time = Int64.Parse(day + hour);                     //获得当前的时间
                int viewnum = 0;
                Debug.WriteLine("count is "+num+" time is "+time);
                //string insertStr = "insert into test(id) values(500)";
                selectStr = "insert into topic (topicid,username,content,topic_date,viewnum,topicname) values("+num+",'"
                    + username +"','"+newtopic+"',"+time+","+0+",'"+newtopicname+"')";
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

            Response.Redirect("/Topic/Index");
            return View();
        }
        //发表新的评论同时也要更新话题的最新时间
        public ActionResult SaveComment(string comment)
        {
            Debug.WriteLine("new commend is " + comment);
            string username = Session["username"] as string;
            OracleConnection conn = new ConnectTheDB().getDb();
            try
            {
                conn.Open();
                string selectStr = "select count (topicid) from topiccomment";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                //Debug.WriteLine("获取数据成功！");
                dr.Read();
                int num = dr.GetInt32(0);
                string day = System.DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "");
                string hour = System.DateTime.Now.ToString("HH:mm:ss").Replace(":", "");
                long time = Int64.Parse(day + hour);                     //获得当前的时间
                int viewnum = 0;
                Debug.WriteLine("count is " + num + " time is " + time);
                //string insertStr = "insert into test(id) values(500)";
                selectStr = "insert into topiccomment (topiccommentid,topicid,content,topiccomment_date,username) values(" + num + ","
                    + topicid + ",'" + comment + "'," + time + ",'" + username + "')";
                OracleCommand c = new OracleCommand(selectStr, conn);
                int insertedLines = c.ExecuteNonQuery();
                selectStr = "update topic set topic_date="+time+"where topicid="+topicid;
                OracleCommand u = new OracleCommand(selectStr, conn);
                insertedLines = u.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取出错！");
            }
            finally
            {
                conn.Close();
            }

            Response.Redirect("/Topic/selecttopic");
            return View();
        }
    }
}