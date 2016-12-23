using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class PlusController : Controller
    {
        // GET: Plus
        public ActionResult Index()
        {
            Session["savexml"] = new SaveXML();
            return View();

        }
        public JsonResult addChar(double[] input, int output)
        {
            double[] Outp = new double[14];
            Outp[output] = 1;
            string dataPath = Server.MapPath("~/App_Data/data.xml");
            ((SaveXML)Session["savexml"]).add(input, Outp,dataPath);
            int a = ((SaveXML)Session["savexml"]).GetCount();
            int[] b = new int[2] { output, a };
            return Json(b);
        }
    }
}