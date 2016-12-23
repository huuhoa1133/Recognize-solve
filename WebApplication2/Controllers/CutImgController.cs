using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class CutImgController : Controller
    {
        // GET: CutImg
        public ActionResult Index()
        {
            //load data base
            string path = Server.MapPath("~/App_Data/RecogCharacterPlus.xml");
            LoadXMLModel load = new LoadXMLModel();
            load.LoadXml(path);
            Session["bpn"] = load;
            return View();
        }
        public JsonResult GetValue(int type, int typew12)//typew12 [0,1] chia w12 ra lan doi
        {
            switch (type)
            {
                case 1:
                    {
                        List<double> w12 = ((LoadXMLModel)Session["bpn"]).GetW12();
                        switch (typew12)
                        {
                            case 1:
                                {
                                    //78400 phan tu dau tien
                                    List<double> w121 = new List<double>();
                                    for (int i = 0; i < 78400; i++)
                                    {
                                        w121.Add(w12[i]);
                                    }
                                    return Json(w121);
                                }
                            case 2:
                                {
                                    //78400 -> 
                                    List<double> w122 = new List<double>(78400);
                                    for (int i = 78400; i < 156800; i++)
                                    {
                                        w122.Add(w12[i]);
                                    }
                                    return Json(w122);
                                }
                            default: return Json(0);
                        }
                    }
                case 2:
                    {

                        List<double> b2 = ((LoadXMLModel)Session["bpn"]).GetBias2();
                        return Json(b2);
                    }
                case 3:
                    {

                        List<double> w23 = ((LoadXMLModel)Session["bpn"]).GetW23();
                        return Json(w23);
                    }
                case 4:
                    {

                        List<double> b3 = ((LoadXMLModel)Session["bpn"]).GetBias3();
                        return Json(b3);
                    }
                default: return Json(0);
            }
        }

        public JsonResult GetResult(string expression)
        {
            TinhGTBT cal = new TinhGTBT();
            string result = cal.Calculate(expression);
            return Json(result);
        }
    }
}