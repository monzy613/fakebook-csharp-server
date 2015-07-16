using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using fakebook.Models;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Timers;
using System.Xml;
using System.Net;

namespace fakebook.Controllers
{
    public class AccountController : Controller
    {
        

        static string oradb = "Data Source=(DESCRIPTION="
             + "(ADDRESS=(PROTOCOL=TCP)(HOST=221.239.198.40)(PORT=1521))"
             + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));"
             + "User Id=c##happy;Password=123456;";


        static int textId = 0;

        public static void dbug(string from, string msg) {
            Debug.WriteLine("[" + from + "]: " + msg);
        }


        

        //发表说说
        public ActionResult AddText(string text)
        {

            //假设登录用户为123，需换为session
            string loginuser = "123";
            //string loginuser = Session["username"] as string
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string selectStr = "select textid from text";
            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleDataReader dr = cmd.ExecuteReader();
            int temp = 0;
            while (dr.Read())
            {
                if (Convert.ToInt32(dr[0]) > temp)
                {
                    temp = Convert.ToInt32(dr[0]);
                }
            }
            temp++;


            string addStr = "insert into text(textid, username, content, text_date, viewnumber)values (" + temp + ",'" + loginuser + "','" + text + "','" + DateTime.Now.ToString() + "',0)";
            OracleCommand cmd2 = new OracleCommand(addStr, conn);
            int insertedLines = cmd2.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Text", "Account");
        }

        //发表评论
        public ActionResult AddComment(string content)
        {

            //假设登录用户为123，需换为session
            string loginuser = "123";
            //string loginuser = Session["username"] as string
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();

            string selectStr = "select textcommentid from textcomment";
            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleDataReader dr = cmd.ExecuteReader();
            int temp = 0;
            while (dr.Read())
            {
                if (Convert.ToInt32(dr[0]) > temp)
                {
                    temp = Convert.ToInt32(dr[0]);
                }
            }
            temp++;

            string addStr = "insert into textcomment(textcommentid, textid, content, textcomment_date, username)values(" + temp + "," + textId + ",'" + content + "','" + DateTime.Now.ToString() + "','" + loginuser + "')";
            OracleCommand cmd2 = new OracleCommand(addStr, conn);
            int insertedLines = cmd2.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Comment", "Account");

        }

        //展示说说状态
        public ActionResult Text()
        {
            //假设登录用户为123，需换为session
            string loginuser = "123";
            //string lgoinuser = Session["username"] as string

            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();

            string selectStr = "select textId,content,text_date,viewnumber,username from text where username ='" + loginuser + "'";
            string selectStr2 = "select friendname from friend where username ='" + loginuser + "'";

            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleDataReader dr = cmd.ExecuteReader();

            OracleCommand cmd2 = new OracleCommand(selectStr2, conn);
            OracleDataReader dr2 = cmd2.ExecuteReader();

            List<textModel> text = new List<textModel>();

            while (dr.Read())
            {
                textModel temp = new textModel();
                temp.textId = Convert.ToInt32(dr[0]);
                temp.content = dr[1].ToString();
                temp.date = dr[2].ToString();
                temp.viewnumber = Convert.ToInt32(dr[3]);
                temp.username = dr[4].ToString();
                text.Add(temp);
            }



            while (dr2.Read())
            {
                selectStr = "select textId,content,text_date,viewnumber,username from text where username ='" + dr2[0].ToString() + "'";
                cmd = new OracleCommand(selectStr, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    textModel temp = new textModel();
                    temp.textId = Convert.ToInt32(dr[0]);
                    temp.content = dr[1].ToString();
                    temp.date = dr[2].ToString();
                    temp.viewnumber = Convert.ToInt32(dr[3]);
                    temp.username = dr[4].ToString();
                    text.Add(temp);
                }
            }
            textModel temp2 = new textModel();
            //List<textModel> text2 = new List<textModel>();
            Debug.WriteLine(text.Count + "test.....");
            for (int i = 0; i < text.Count - 1; i++)
            {
                //Debug.WriteLine(temp2.content);
                //temp2 = text.ElementAt(i);
                for (int j = i + 1; j < text.Count; j++)
                {
                    if (text.ElementAt(i).date.CompareTo(text.ElementAt(j).date) < 0)
                    {
                        temp2 = text.ElementAt(j);
                        text[j] = text[i];
                        text[i] = temp2;
                    }
                }
            }

            ViewData["text"] = text;
            conn.Close();
            return View();//反馈到查看个人用户说说的页面
        }


        //展示说说的评论
        public ActionResult Comment(string textid, string content)
        {
            if (textid != null)
                textId = Convert.ToInt32(textid);
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            // Debug.WriteLine(textId);
            string updateStr = "update text set viewnumber=viewnumber+1 where textid=" + textId;
            OracleCommand cmd2 = new OracleCommand(updateStr, conn);
            int insertedLines = cmd2.ExecuteNonQuery();

            string selectStr = "select content,textcomment_date,username from textcomment where textId = " + textId;
            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleDataReader dr = cmd.ExecuteReader();

            List<CommentbyIdModel> comment = new List<CommentbyIdModel>();
            while (dr.Read())
            {
                CommentbyIdModel temp = new CommentbyIdModel();
                temp.content = dr[0].ToString();
                temp.date = dr[1].ToString();
                temp.userName = dr[2].ToString();
                comment.Add(temp);
            }
            ViewData["comment"] = comment;
            conn.Close();
            return View();

        }






        // GET: Account/Login
        public ActionResult Login()

        {
            ViewBag.icon = "~/Portraits/" + "default.jpg";
            return View();
        }



        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string selectStr = "select * from fake_user where username  ='"
                    + username + "' and password = '"
                    + password + "'";
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();



                bool findUser = dr.Read();
                if (findUser)
                {
                    string dbusername = dr.GetOracleString(0).ToString();
                    string dbnickname = dr.GetOracleString(2).ToString();
                    string dbheadimg = dr.GetOracleString(10).ToString();
                    Session.Add("username", username);
                    Session.Add("nickname", dbnickname);
                    Session.Add("headimg", dbheadimg);
                    
                    
                    dbug("Login", "success");
                    dr.Close();
                    return RedirectToAction("UserPage", "Account");
                }
                else
                {
                    dr.Close();
                    return View();
                }
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
                return View();
                //return "other exception: " + ex.Message.ToString();
            }
            finally {
                conn.Close();
                conn.Dispose();
            }
        }

        public ActionResult Test() {
            return View();
        }

        


        // GET: Account/Register
        public ActionResult Register() {
            return View();
        }

        [HttpPost]
        public string Register(string username, string password, string nickname, string email)
        {

            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string insertStr = "insert into fake_user(username, password, nickname,  email, state) values('"
                                 + username + "', '"
                                 + password + "', '"
                                 + nickname + "', '"
                                 + email + "', 2)";
                OracleCommand cmd = new OracleCommand(insertStr, conn);
                //OracleDataReader dr = cmd.ExecuteReader();
                //dr.Read();
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                ViewBag.username = username;
                ViewBag.password = password;
                Response.Redirect("Login");
                //             return ("insertedLines: " + insertedLines);
            }
            catch (OracleException ex) // catches only Oracle errors
            {
                switch (ex.Number)
                {
                    case 1:
                        return "Error attempting to insert duplicate data.";
                    case 12545:
                        return "The database is unavailable.";
                    default:
                        return "Database error: " + ex.Message.ToString();
                }
            }
            catch (Exception ex) // catches any error not previously caught
            {
                return "other expectation";
            }
            Console.WriteLine("success");

            return "nothing";
        }


        public ActionResult UserPage() {
            string currentUser = Session["username"] as string;
            if (currentUser != null && !currentUser.Equals(""))
            {
                ViewBag.username = currentUser;
                ViewBag.headimg = "~/Portraits/" + (Session["headimg"] as string);
                OracleConnection conn = new OracleConnection(oradb);
                string friendGetStr = "select u.username, u.nickname from fake_user u, friend f where f.username = '"
                                    + currentUser
                                    + "' and f.friendname = u.username";
                OracleCommand cmd = new OracleCommand(friendGetStr, conn);
                List<FriendModel> friendList = new List<FriendModel>();
                try
                {
                    conn.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read()) {
                        string _id = dr.GetOracleString(0).ToString();
                        string nickname = dr.GetOracleString(1).ToString();
                        string json = "{\"_id\":\""
                                    + _id
                                    + "\",\"nickname\":\""
                                    + nickname
                                    + "\"}";
                        FriendModel model = new FriendModel();
                        model._id = _id;
                        model.nickname = nickname;
                        Debug.WriteLine("Friend: " + json);
                        friendList.Add(model);
                    }
                    ViewData["friendList"] = friendList;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex);
                }
                finally {
                    conn.Close();
                    conn.Dispose();
                }
                return View();
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult UserPage(string username, string password) {
            OracleConnection conn = new OracleConnection(oradb);
            try {
                conn.Open();
                string loginSearch = "select * from fake_user where username = "
                                   + "'" + username + "' and password = "
                                   + "'" + password + "'";
                OracleCommand cmd = new OracleCommand(loginSearch, conn);
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
                        return View();                        //return "Database error: " + ex.Message.ToString();
                }
            }
            catch (Exception ex) // catches any error not previously caught
            {
                Console.Write("Other exception");
            }
            return View();
        }

        public ActionResult Chatroom() {
            ViewBag.nickname = Session["nickname"] as string;
            ViewBag.username = Session["username"] as string;
            return View();
        }


        //friend
        public ActionResult Search(string type, string information)
        {

            //假设登录用户为123，需换为session
            string loginuser = Session["username"] as string;
            if (information == null)
                return View();

            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();

            string selectStr = "select username,nickname,gender from fake_user where " + type + " like'%" + information + "%'";

            string selectStr_friend = "select friendname from friend where username = '" + loginuser + "'";

            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleCommand cmd2 = new OracleCommand(selectStr_friend, conn);
            OracleDataReader dr = cmd.ExecuteReader();
            OracleDataReader dr2 = cmd2.ExecuteReader();

            List<SearchModel> find = new List<SearchModel>();
            List<string> friend = new List<string>();
            while (dr2.Read())
            {
                friend.Add(dr2[0].ToString());
            }
            while (dr.Read())
            {
                SearchModel searchModel = new SearchModel();
                if (!loginuser.Equals(dr[0].ToString()))
                {
                    searchModel.username = dr[0].ToString();
                    searchModel.nickname = dr[1].ToString();
                    searchModel.gender = dr[2].ToString();
                    searchModel.beFriend = false;
                    for (int i = 0; i < friend.Count; i++)
                    {
                        if (dr[0].ToString().Equals(friend.ElementAt(i)))
                            searchModel.beFriend = true;
                    }
                    //Debug.WriteLine(searchModel.username);
                    // Debug.WriteLine(searchModel.beFriend);
                    find.Add(searchModel);
                }
            }
            ViewData["find"] = find;
            conn.Close();
            return View();
        }

        //public ActionResult Add()
        //{
        //    return RedirectToAction("Search","Account");
        //}


        //添加好友
        public ActionResult Add(string friendname)
        {
            string loginuser = Session["username"] as string;
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string addStr = "insert into friend(username, friendname, homeauthority, galleryauthority)values('" + loginuser + "', '" + friendname + "', 1, 1)";
            string addStr2 = "insert into friend(username, friendname, homeauthority, galleryauthority)values('" + friendname + "', '" + loginuser + "', 1, 1)";
            OracleCommand cmd = new OracleCommand(addStr, conn);
            OracleCommand cmd2 = new OracleCommand(addStr2, conn);
            int insertedLines = cmd.ExecuteNonQuery();
            int insertedLines2 = cmd2.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Search", "Account");
        }


        //删除好友
        public ActionResult Del(string friendname)
        {
            //假设登录用户为123,需换为session
            string loginuser = Session["username"] as string;
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string addStr = "delete from friend where username = '" + loginuser + "' and friendname = '" + friendname + "'";
            string addStr2 = "delete from friend where username = '" + friendname + "' and friendname = '" + loginuser + "'";
            OracleCommand cmd = new OracleCommand(addStr, conn);
            OracleCommand cmd2 = new OracleCommand(addStr2, conn);
            int insertedLines = cmd.ExecuteNonQuery();
            int insertedLines2 = cmd2.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Search", "Account");
        }



        public ActionResult PeopleAround() {
            OracleConnection conn = new OracleConnection(oradb);
            string currentUser = Session["username"] as string;
            if (currentUser != null && !currentUser.Equals("")) {

                try
                {
                    conn.Open();
                    Stack<LocationModel> peopleStack = new Stack<LocationModel>();
                    string selectStr = "select * from location where username != '" + currentUser + "'";
                    OracleCommand cmd = new OracleCommand(selectStr, conn);
                    OracleDataReader dr = cmd.ExecuteReader();
                    string peopleListStr = "";
                    int count = 0;
                    while (dr.Read())
                    {
                        LocationModel md = new LocationModel(dr[0].ToString(),
                                                             dr.GetDouble(1),
                                                             dr.GetDouble(2));
                        peopleStack.Push(md);
                        Debug.WriteLine(md);
                        peopleListStr = peopleListStr + md.username + " " + md.longitude + " " + md.latitude + " ";
                        ++count;
                    }
                    ViewBag.count = count;
                    ViewBag.peopleStack = peopleStack;
                    ViewBag.peopleListStr = peopleListStr.Substring(0, peopleListStr.Length - 1);
                    Debug.WriteLine("|" + peopleListStr.Substring(0, peopleListStr.Length - 1) + "|");
                    return View();
                }
                catch (Exception ex) {
                    Debug.WriteLine("EX[PeopleAround] " + ex);
                    return RedirectToAction("UserPage", "Account");
                }
                
            }
            else {
                return RedirectToAction("Fakebook", "WebView");
            }
        }

        public void AddFromMap(string friendname)
        {
            Debug.WriteLine("Add From Map: " + friendname);
            string loginuser = Session["username"] as string;
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string addStr = "insert into friend(username, friendname, homeauthority, galleryauthority)values('" + loginuser + "', '" + friendname + "', 1, 1)";
            string addStr2 = "insert into friend(username, friendname, homeauthority, galleryauthority)values('" + friendname + "', '" + loginuser + "', 1, 1)";
            OracleCommand cmd = new OracleCommand(addStr, conn);
            OracleCommand cmd2 = new OracleCommand(addStr2, conn);
            int insertedLines = cmd.ExecuteNonQuery();
            int insertedLines2 = cmd2.ExecuteNonQuery();
            conn.Close();
        }


        [HttpPost]
        public void SendLocation(double longitude, double latitude) {
            OracleConnection conn = new OracleConnection(oradb);

            try{
                conn.Open();
                string selectStr = "select * from location where username = '" + (Session["username"] as string) + "'";
                Debug.WriteLine(selectStr);
                OracleCommand selectCMD = new OracleCommand(selectStr, conn);
                OracleDataReader dr = selectCMD.ExecuteReader();
                if (dr.Read())
                {

                    string loginuser = Session["username"] as string;
                    //string delStr = "delete from location where username = '" + loginuser + "'";
                    /*
                    string updateStr = "update location set longitude = " + longitude + ", latitude = " + latitude + " where username = " + loginuser;
                    OracleCommand cmd = new OracleCommand(updateStr, conn);
                    int insertedLines = cmd.ExecuteNonQuery();
                    conn.Close();*/

                    string delStr = "delete from location where username = '" + loginuser + "'";
                    OracleCommand cmd = new OracleCommand(delStr, conn);
                    int deletedLines = cmd.ExecuteNonQuery();
                    Debug.WriteLine("[DELETE]");

                    string insertStr = "insert into location(username, longitude, latitude) values('"
                                     + (Session["username"] as string)
                                     + "', "
                                     + longitude
                                     + ", "
                                     + latitude
                                     + ")";
                    OracleCommand insertCMD = new OracleCommand(insertStr, conn);
                    int insertedLines = insertCMD.ExecuteNonQuery();
                    Debug.WriteLine("Insert: " + insertStr + " " + insertedLines);
                    conn.Close();
                    conn.Dispose();


                    //delete
                    //delete from location where username = '123';
                    //string deleteStr = "delete from location where username = '"
                    //                 + (Session["username"] as string)
                    //                 + "'";
                    /*
                    string deleteStr = "delete from location where username = '123'";
                    Debug.WriteLine("1.Delete: " + deleteStr);
                    OracleCommand deleteCMD = new OracleCommand(deleteStr, conn);
                    deleteCMD.ExecuteNonQueryAsync();
                    deleteCMD.Dispose();
                    Debug.WriteLine("2.Delete: " + deleteStr);
                    */



                    //update
                    //update location set longitude = 333, latitude = 222 where username = '123';
                    /*
                    OracleCommand updateCMD = conn.CreateCommand();
                    updateCMD.CommandType = System.Data.CommandType.Text;
                    updateCMD.CommandText = "update location set longitude=111111,latitude=111111 where username='123'";
                    updateCMD.ExecuteNonQuery();
                    Debug.WriteLine("update:  " + updateCMD.CommandText);
                    */
                   // Debug.WriteLine("3.Update: " + updateStr);
                    

                }
                else
                {
                    //insert new
                    //insert into location(username, longitude, latitude) values('123', 123, 234)
                    string insertStr = "insert into location(username, longitude, latitude) values('"
                                     + (Session["username"] as string)
                                     + "', "
                                     + longitude
                                     + ", "
                                     + latitude
                                     + ")";
                    OracleCommand insertCMD = new OracleCommand(insertStr, conn);
                    int insertedLines = insertCMD.ExecuteNonQuery();
                    Debug.WriteLine("Insert: " + insertStr + " " + insertedLines);
                    conn.Close();
                    conn.Dispose();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("[SEND LOCATION Exception: " + ex);
            }
            finally {



            }
            Debug.WriteLine("Longitude: " + longitude + ", Latitude: " + latitude);
        }




        // [HttpPost]
        public string AddPoints()             //签到加分
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string insertStr = "update fake_user "
                     + "set points = points + 5 "
                    + "where userName = '" + (Session["username"] as string) + "'";
                OracleCommand cmd = new OracleCommand(insertStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();

                string selectStr = "select points "
                + "from fake_user "
                + "where userName = '" + (Session["username"] as string) + "'";
                cmd = new OracleCommand(selectStr, conn);
                insertedLines = cmd.ExecuteNonQuery();
                OracleDataReader dr = cmd.ExecuteReader();
                dr.Read();
                int currentPoints = dr.GetInt32(0);
                conn.Close();
                conn.Dispose();
                return "AddPonints:" + (Session["username"] as string) + "+ 5" + "    NOW:" + currentPoints;
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }

        /*
        public ActionResult Modify()
        {
            string currentUsername = Session["username"] as string;
            if (currentUsername != null && !currentUsername.Equals("")) {
            }
            else {
                return RedirectToAction("UserPage");
            }
            return View();
        }



        [HttpPost]
        public String Modify(string nickname, string password, string email, char gender)           //个人信息修改 date类型未定，生日暂时不改
        {

            OracleConnection conn = new OracleConnection(oradb);

            try
            {
                conn.Open();
                string updateStr = "update fake_user "
                      + " set nickname = '" + nickname + "' , "
                      + " password = '" + password + "' , "
                      + " email = '" + email + "' , "
                      + " gender = '" + gender + "' "
                      + " where username = '" + (Session["username"] as string) + "'";
                OracleCommand cmd = new OracleCommand(updateStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return "update:" + (Session["username"] as string);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return "exception   ";
            }
        }


        */

        public ActionResult Modify()
        {
            string loginuser = Session["username"] as string;
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string selectStr = "select nickname, gender, phonenumber, email, tag from fake_user where username ='" + loginuser + "'";
            OracleCommand cmd = new OracleCommand(selectStr, conn);
            OracleDataReader dr = cmd.ExecuteReader();

            List<informationModel> inf = new List<informationModel>();

            informationModel information = new informationModel();

            if (dr.Read())
            {
                information.nickname = dr[0].ToString();
                information.gender = dr[1].ToString();
                information.tel = dr[2].ToString();
                information.email = dr[3].ToString();
                information.hobby = dr[4].ToString();
                inf.Add(information);
            }

            ViewData["information"] = inf;
            conn.Close();
            return View();
        }



        //个人信息修改
        public ActionResult ModifyConfirm(string nickname, string gender, string tel, string hobby, string email)
        {
            //假设登录用户为123，需换为session
            string loginuser = "123";
            //string loginuser = Session["username"] as string
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            string updateStr = "update fake_user set nickname = '" + nickname + "', gender ='" + gender + "', phonenumber=" + Convert.ToInt32(tel) + ", tag='" + hobby + "', email = '" + email + "'where username = '" + loginuser + "'";

            OracleCommand cmd = new OracleCommand(updateStr, conn);
            int insertedLines = cmd.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("UserPage", "Account");
        }









        public ActionResult Forget()
        {
            return View();
        }
        [HttpPost]
        public string Forget(string username)             //忘记密码
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string selectStr = "select email,password "
               + "from fake_user "
               + "where userName = '" + username + "'";  //忘记密码时没有登录
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();

                OracleDataReader dr = cmd.ExecuteReader();
                dr.Read();
                string TO = dr.GetOracleString(0).ToString();      //目标地址
                string PASSWORD = dr.GetOracleString(1).ToString();
                conn.Close();
                conn.Dispose();

                string smtpServer = "smtp.qq.com"; //SMTP服务器
                string mailFrom = "2892782011@qq.com"; //登陆用户名
                string userPassword = "mufei123";//登陆密码

                // 邮件服务设置
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
                smtpClient.Host = smtpServer; //指定SMTP服务器
                smtpClient.Port = 25;
                smtpClient.Credentials = new System.Net.NetworkCredential(mailFrom, userPassword);//用户名和密码

                // 发送邮件设置        
                MailMessage mailMessage = new MailMessage(mailFrom, TO); // 发送人和收件人
                mailMessage.Subject = "Fakebook密码找回服务";//主题
                mailMessage.Body = "密码找回成功！ " + "用户" + username + "      您的密码是：" + PASSWORD;//内容
                mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
                mailMessage.IsBodyHtml = true;//设置为HTML格式
                mailMessage.Priority = MailPriority.Low;//优先级
                //System.Diagnostics.Debug.WriteLine("haiwefasong ,please waiting-----");
                smtpClient.Send(mailMessage); // 发送邮件
                // Console.WriteLine("success!.."); 






                return "EMAIL SENT:";
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }




    }
}