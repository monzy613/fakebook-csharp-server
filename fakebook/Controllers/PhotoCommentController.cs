using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using fakebook.Models;
using System.Diagnostics;

namespace fakebook.Controllers
{
    public class PhotoCommentController : Controller
    {
        //GET: \PhotoComment\PhotoShow
        //点击照片后跳到这张照片的展示页面，下面是所有评论的展示
        
        public ActionResult ReadComment()
        {
            string photoId;
            OracleConnection conn = new ConnectTheDB().getDb();
            conn.Open();
            if (Request.Form["photoid"] != null) 
            { 
                photoId = Request.Form["photoid"];
                Session.Add("currentPhotoId", photoId);
            }
            else
            {
                photoId = Session["currentPhotoId"] as string;
            }
            if (Request.Form["state"] != null)
            {
                string userName = Session["username"] as string;
                //@@@@
                Debug.WriteLine(userName);
                string photoComment_date = DateTime.Now.ToString();
                //@@@@
                Debug.WriteLine(photoComment_date);
                photoId = Session["currentPhotoId"] as string;
                //@@@@
                Debug.WriteLine(photoId);
                string content = Request.Form["state"];
                //@@@@
                Debug.WriteLine(content);
                try
                {
                    string selectStr = "select photoCommentId from photoComment";
                    //@@@@
                    Debug.WriteLine("lalala+4");
                    OracleCommand cmd = new OracleCommand(selectStr, conn);
                    OracleDataReader dr2 = cmd.ExecuteReader();
                    //@@@@
                    Debug.WriteLine("lalala+5");
                    int temp = 0;
                    while (dr2.Read())
                    {
                        if (Convert.ToInt32(dr2[0]) > temp)
                        {
                            temp = Convert.ToInt32(dr2[0]);
                        }
                    }
                    temp++;
                    string photoCommentId = temp.ToString();
                    //@@@@
                    Debug.WriteLine(photoCommentId);
                    string insertStr = "insert into photoComment(photoCommentId, photoId, content, photoComment_date, userName) values('"
                        + photoCommentId + "','"
                        + photoId + "','"
                        + content + "','"
                        + photoComment_date + "','"
                        + userName + "')";
                    //@@@@
                    Debug.WriteLine("lalala+7");
                    OracleCommand cmd2 = new OracleCommand(insertStr, conn);
                    //@@@@
                    Debug.WriteLine("lalala+8");
                    int insertedLines = cmd2.ExecuteNonQuery();
                    //@@@@
                    Debug.WriteLine("lalala+9");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("向数据库写入数据出错！");
                }
            }
            try
            {
                
               
                string selectStr = "select * from photoComment where photoId = "+ photoId ;
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                List<photoComment> pc = new List<photoComment>();
                while (dr.Read())                        //逐行读取数据库中的记录
                {
                    photoComment pcItem = new photoComment();
                    pcItem.photoCommentId = dr.GetString(0);
                    pcItem.photoId = dr.GetString(1);
                    pcItem.content = dr.GetString(2);
                    pcItem.photoComment_date = dr.GetString(3);
                    pcItem.userName = dr.GetString(4);
                    pc.Add(pcItem);
                }
                ViewData["photoId"] = photoId;
                ViewData["comment"] = pc;                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取相册评论出错！");
            }
            string selectStr1 = "select * from photo where photoid=" + photoId;
            OracleCommand cmd1 = new OracleCommand(selectStr1, conn);
            OracleDataReader dr1 = cmd1.ExecuteReader();
            dr1.Read();
            ViewData["photoAddress"] = dr1["photoaddress"].ToString();

            //@@@@
            Debug.WriteLine(Request.Form["state"]);

            conn.Close();
            return View();
        }



        public ActionResult test()
        {
            OracleConnection conn = new ConnectTheDB().getDb();

            try
            {
                conn.Open();



                string insertStr = "insert into photoComment(photoCommentId, photoId, content, photoComment_date, userName) values('2', '1', '么么哒', '2015-07-16', 'lalala')";
                OracleCommand cmd1 = new OracleCommand(insertStr, conn);
                cmd1.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("向数据库写入数据出错！");
            }

            try
            {
                string selectStr = "select * from photoComment where photoId = '1'";
                OracleCommand cmd2 = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd2.ExecuteReader();
                List<photoComment> pc = new List<photoComment>();

                while (dr.Read())                        //逐行读取数据库中的记录
                {
                    photoComment pcItem = new photoComment();
                    pcItem.photoCommentId = dr.GetString(0);
                    pcItem.photoId = dr.GetString(1);
                    pcItem.content = dr.GetString(2);
                    pcItem.photoComment_date = dr.GetString(3);
                    pcItem.userName = dr.GetString(4);
                    pc.Add(pcItem);
                }
                ViewData["photoId"] = 1;
                ViewData["comment"] = pc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取相册评论出错！");
            }

            conn.Close();
            conn.Dispose();

            return View();
        }
    }

}