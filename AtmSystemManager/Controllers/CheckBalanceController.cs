using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Models;
using AtmSystemManager.Controllers.Progressor;

namespace AtmSystemManager.Controllers
{
    public class CheckBalanceController : Controller
    {
        // GET: /CheckBalance/

        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: DatLH - Review Codeding Convention: ManhNV
        /// </summary>

        public ActionResult Index()
        {
            Session["Error2"] = "";
            var hv = new HistoryView();
            var cardNo = Session["CardNo"].ToString();
            var tempMoney = MoneyBalance(cardNo);
            ViewBag.Balance = hv.ChairProgress(tempMoney);
            return View();
        }

        public decimal MoneyBalance(string cardNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var card = dataEntities.Cards;
                var account = dataEntities.Accounts;

                var infor = (from cd in card
                             join ac in account
                             on cd.AccountID equals ac.AccountID
                             //join lg in logTemp
                             //on cd.CardNo equals lg.CardNo
                             where cd.CardNo.Equals(cardNo)
                             select new { ac.Balance }).ToList();

                var moneyBalance = infor[0].Balance;
                return moneyBalance < 50000 ? (50000) : moneyBalance;
            }
        }

        [HttpPost]
        public ActionResult NoReport()
        {
            return RedirectToAction("CheckTransaction", "Withdraw");
        }

        [HttpPost]
        public ActionResult Report()
        {
            return RedirectToAction("CheckTransaction", "Withdraw");
        }
    }
}
