using System.Web.Mvc;

namespace AtmSystemManager.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/

        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: ManhNV - Review Codeding Convention: ManhNV
        /// </summary>

        public ActionResult Index()
        {
            return Session["CardNo"] == null
                ? RedirectToAction("InputCard", "Validation")
                : ((string)Session["CardNo"] == ""
                    ? (ActionResult)RedirectToAction("InputCard", "Validation")
                    : View());
        }
    }
}
