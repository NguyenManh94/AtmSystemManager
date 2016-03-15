using System;
using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Models;

namespace AtmSystemManager.Controllers
{
    public class HistoryController : Controller
    {
        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: DongLK - Review Codeding Convention: ManhNV
        /// </summary>

        // GET: /History/
        readonly ManagerATMEntities _atm = new ManagerATMEntities();

        public ActionResult Index()
        {
            SetSessionView();
            if (Session["day"] != null && Session["id"] != null) return View();
            Session["day"] = "0";
            Session["id"] = "0";

            return View();
        }

        [HttpPost]
        public ActionResult Check(int check)
        {
            if (check.Equals(1))
            {
                return RedirectToAction("Index", "Home");
            }
            if (check.Equals(2))
            {
                return RedirectToAction("Index", "History");
            }
            return View();
        }

        public void SetSessionView()
        {
            Session["day"] = "0";
            Session["id"] = "0";
        }

        public ActionResult ShowHistory(int day = 0, int id = 0)
        {
            try
            {
                var dayz = int.Parse(Session["day"].ToString()) + day;
                Session["day"] = dayz;
                var idz = int.Parse(Session["id"].ToString()) + id;

                var cardNo = Session["CardNo"].ToString();
                var date = DateTime.Now;
                var datez = date.AddDays(-dayz); /*Gend code datetimenow - number*/

                var logs = from lg in _atm.Logs where (lg.CardNo.Equals(cardNo) && lg.LogDate >= datez) orderby lg.LogID descending select lg ;
                var l = logs.ToList();
                l.Clear();
                var a = logs.Count();
                if (idz >= a)
                {
                    idz = idz - id;
                }
                else if (idz < 0)
                {
                    idz = idz - id;
                }
                Session["id"] = idz;
                for (int i = idz; i < idz + 5; i++)
                {
                    if (i >= a)
                    {
                        break;
                    }
                    l.Add(logs.ToList().ElementAt(i));
                }
                return View(l);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
