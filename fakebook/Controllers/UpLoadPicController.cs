using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace fakebook.Controllers
{
    public class UpLoadPicController : Controller
    {
        // GET: UpLoadPic
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult ShowPic()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UpPic()
        {
            HttpPostedFileBase file = Request.Files["picture"];
            if (file != null)
            {
                string filename = Path.GetFileName(file.FileName);
                string[] names = filename.Split('.');
                string type = names[1];
                string newfilename = "11312" + "." + type;
                string filepath = Path.Combine(HttpContext.Server.MapPath("../Portraits"), newfilename);
                Debug.WriteLine("file name is " + newfilename );
                file.SaveAs(filepath);
                TempData["picturesrc"] = "../Portraits/"+newfilename;
                return RedirectToAction("ShowPic", "UpLoadPic");
            }else
            {
                Debug.WriteLine("上传失败");
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChangeHeadImage() {
            string username = Session["username"] as string;
            HttpPostedFileBase file = Request.Files["picture"];
            if (file != null)
            {
                string filename = Path.GetFileName(file.FileName);
                string[] names = filename.Split('.');
                string type = names[1];
                string newfilename = username + "." + type;

                OracleConnection conn = new ConnectTheDB().getDb();

                try
                {
                    conn.Open();
                    string updateStr = "update fake_user "
                          + " set headimg = '"
                          + newfilename
                          + "' where username = '" + (Session["username"] as string) + "'";
                    OracleCommand cmd = new OracleCommand(updateStr, conn);
                    int insertedLines = cmd.ExecuteNonQuery();
                    Session.Add("headimg", newfilename);
                    conn.Close();
                    conn.Dispose();
                    return Upload(0, newfilename, file);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return RedirectToAction("UserPage", "Account");
                }
                
            }
            else
            {
                Debug.WriteLine("上传失败");
                return RedirectToAction("UserPage", "Account");
            }
        }


        /*
     [HttpPost]
        public ActionResult UpPic()
        {
            HttpPostedFileBase file = Request.Files["picture"];
            if (file != null)
            {
                string filename = Path.GetFileName(file.FileName);
                string[] names = filename.Split('.');
                string type = names[1];
                string newfilename = "11312" + "." + type;
                string filepath = Path.Combine(HttpContext.Server.MapPath("../Portraits"), newfilename);
                Debug.WriteLine("file name is " + newfilename );
                file.SaveAs(filepath);
                TempData["picturesrc"] = "../Portraits/"+newfilename;
                return RedirectToAction("ShowPic", "UpLoadPic");
            }else
            {
                Debug.WriteLine("上传失败");
                return View();
            }
        }          
         */


        //fileType is 0 or 1
        //0 is upload headImg, secondPara is headimage filename
        //1 is upload photo, secondPara is albumname
        //
        private ActionResult Upload(int fileType, string secondPara, HttpPostedFileBase file) {
            if (fileType == 0)
            {
                //headimage
                string filename = secondPara;
                string filepath = Path.Combine(HttpContext.Server.MapPath("../Portraits"), filename);
                Debug.WriteLine("file name is " + filename);
                file.SaveAs(filepath);
                return RedirectToAction("UserPage", "Account");
            }
            else {
                return RedirectToAction("Gallery", "Album");
            }
        }
    }
}