using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VedioWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult VideoView()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            string fileName = System.IO.Path.GetFileName(uploadFile.FileName);
            string filePhysicalPath = Server.MapPath("~/upload/" + fileName);
            string pic = "", error = "";
            try
            {
                uploadFile.SaveAs(filePhysicalPath);
                VideoConverter.Clear();
                bool success = VideoConverter.Tans2Mp4(filePhysicalPath);
                if (success)
                {
                    string destFile = VideoConverter.GetImage(filePhysicalPath);
                    pic = "/upload/" + Path.GetFileName(destFile);
                }
                else
                {
                    error = "转换失败！";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return Json(new
            {
                pic = pic,
                error = error
            });
        }
    }
}