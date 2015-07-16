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
    public class AlbumController : Controller
    {

        static string oradb = "Data Source=(DESCRIPTION="
             + "(ADDRESS=(PROTOCOL=TCP)(HOST=221.239.198.40)(PORT=1521))"
             + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));"
             + "User Id=C##happy;Password=123456;";
        // GET: Album
      
      //  [HttpPost]
        public List<AlbumModels> Album()
        {
            OracleConnection conn = new OracleConnection(oradb);
        
                conn.Open();
                string selectStr =   /* "insert into album(albumID,albumname,username) values('111','testalbum','KING')";*/
                                       "select * "
                                       + "from album ";
                    // " +(Session["username"] as string)+"

                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                var Models = new List<AlbumModels>();
                while (dr.Read())
                { 
 
                    AlbumModels model = new AlbumModels();
                    model.AlbumName = dr["albumname"].ToString();
                    model.AlbumId = dr["albumid"].ToString();
                    //model.Username = Session["username"] as string ;                    
                    string selectStr1 = "select * from photo where albumid = " + model.AlbumId;
                    OracleCommand cmd1 = new OracleCommand(selectStr1, conn);
                    OracleDataReader dr1 = cmd1.ExecuteReader();
                    dr1.Read();
                    model.Firstphoto = dr1["photoaddress"].ToString();
                    Models.Add(model);
                    //Session.Add("currentAlbumName", albumName);
                    //Session.Add("currentAlbumId", albumId);         //点击时保存到session
                }
                dr.Close();
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return Models;
        }
    
        public List<PhotoModels> Photo(string id)
        {
            OracleConnection conn = new OracleConnection(oradb);
            var photos = new List<PhotoModels>();
     
                conn.Open();
                string selectStr = "select * from photo where albumid ="+id; //获取当前相册的ID
            //"+(Session["currentAlbumId"] as string) +"
                OracleCommand cmd = new OracleCommand(selectStr, conn);
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var photo = new PhotoModels();
                    photo.PhotoId = dr["photoid"].ToString();
                    photo.PhotoAddress = dr["photoaddress"].ToString();
                    photos.Add(photo);
                }
                conn.Close();
                
                return photos;

        }


        public string CreateAlbum(string id,string name)           //建立相册
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string insertStr = "insert into album(albumId,albumName,username) values('"+id+"','"
                    +name+"','"
                    +(Session["username"] as string)+"')";
                OracleCommand cmd = new OracleCommand(insertStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return "new ALBUM:" + (Session["username"] as string);
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }

        public string AddPhoto(string id,string address)             //新增照片
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string insertStr = "insert into photo(photoId,albumId,photoAddress) values('"+ id+"','"
                    +(Session["currentAlbumId"] as string) + "','"
                    +address +"')";
                OracleCommand cmd = new OracleCommand(insertStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return "new PHOTO:" + (Session["username"] as string);
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }

        public string DeletePhoto(string deleteid)             //删除照片
        {
            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();
                string deleteStr = "delete from photo "
                                   +"where photoid = '"
                                   +deleteid + "'";
                OracleCommand cmd = new OracleCommand(deleteStr, conn);
                int insertedLines = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return "new PHOTO:" + (Session["username"] as string);
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }
        
        public ActionResult Photoshow()
        {
            var id = Request.Form["id"];
            var photos = Photo(id);
            ViewBag.photos = photos;
            return View();
        }
        public ActionResult Gallery()
        {
            var Models = Album();
            ViewBag.models = Models;
            //ViewBag.first = Photos.First();
            return View();
        }














    }
}