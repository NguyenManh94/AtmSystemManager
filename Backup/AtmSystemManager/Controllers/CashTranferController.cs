using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;

namespace AtmSystemManager.Controllers
{
    public class CashTranferController : Controller
    {
        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: NamNH - Review Codeding Convention: ManhNV
        /// </summary>
        /// 
        readonly ManagerATMEntities _db = new ManagerATMEntities();
        //Set begin Session in action 1, View Rules of ATM transfer

        public ActionResult Transfer01()
        {
            Session["ID"] = "";
            Session["Name"] = "";
            Session["Number"] = "";
            Session["Amount"] = "";
            Session["NameTransfer"] = "";
            Session["AccountNoTransfer"] = "";
            Session["demsolan"] = "1";
            return View();
        }

        //Show Senter's Information
        public ActionResult Transfer02()
        {
            var test = Session["CardNo"].ToString();
            {
                var cd = _db.Cards.First(t => t.CardNo.Equals(test));
                var acTransfer = _db.Accounts.Single(accId => accId.AccountID.Equals(cd.AccountID));
                var cusTransfer = _db.Customers.Single(cus => cus.CustID.Equals(acTransfer.CustID));
                Session["AccountNoTransfer"] = acTransfer.AccountNo;
                ViewBag.No = Session["AccountNoTransfer"].ToString();
                Session["NameTransfer"] = cusTransfer.Name;
                ViewBag.NameTransf = Session["NameTransfer"].ToString();
                return View();
            }
        }

        //Check Number account recieve
        [HttpPost]
        public ActionResult Transfer02(string idRecieve)
        {
            var test = Session["CardNo"].ToString();
            if (!CheckDuplicate(test, idRecieve))
            {
                ViewBag.No = Session["AccountNoTransfer"].ToString();
                ViewBag.NameTransf = Session["NameTransfer"].ToString();
                ViewBag.AccountErr = "You can't self transfer! Please re-enter account number recieve";
                return View();
            }
            var acRe = (from ac in _db.Accounts where ac.AccountNo.Equals(idRecieve) select ac);
            if (acRe.Any())
            {
                Session["ID"] = idRecieve;
                return RedirectToAction("Transfer03");
            }
            Session["ID"] = "";
            return RedirectToAction("AccountNotExist");
        }

        //Show reciever's information to view
        public ActionResult Transfer03()
        {
            var recieveAccount = Session["ID"].ToString();
            var ac = _db.Accounts.Single(a => a.AccountNo.Equals(recieveAccount));
            var cs = _db.Customers.Single(kh => kh.CustID.Equals(ac.CustID));

            ViewBag.AccountNo = ac.AccountNo;
            Session["Name"] = cs.Name;
            Session["Number"] = ac.AccountNo;
            return View(cs);
        }

        public ActionResult Transfer04()
        {
            return View();
        }

        //Take amount customer input and check it
        [HttpPost]
        public ActionResult Transfer04(string txtAmount)
        {

            if (txtAmount.ToString() == "")
            {
                ViewBag.err = "Please input amount before press Enter button!";
                return View();

            }
            else
            {
                ViewBag.err = "";
                var test = Session["CardNo"].ToString();
                var amount = decimal.Parse(txtAmount);
                var sentAccount = Session["AccountNoTransfer"].ToString();
                var ac = _db.Accounts.Single(a => a.AccountNo.Equals(sentAccount));
                if (amount > 50000000)
                {
                    Session["Amount"] = "";
                    ViewBag.err = "Transfer maximum 50.000.000 VNĐ once time.";
                    return View();
                }
                if (amount > (ac.Balance - 51000))
                {
                    Session["Amount"] = "";
                    ViewBag.err = "Your account isn't enough to transfer.Re-enter amount you want to transfer! ";
                    return View();
                }
                if (amount % 50000 != 0)
                {
                    Session["Amount"] = "";
                    ViewBag.err = "Amount transfer must is multiples of 50.000 VNĐ ";
                    return View();
                }
                if (!MoneyOverTransfer(test, amount))
                {
                    Session["Amount"] = "";
                    ViewBag.err = "Transfer maximum 100.000.000 VNĐ per day.! ";
                    return View();
                }
                Session["Amount"] = amount;
                return RedirectToAction("Transfer05");
            }
        }

        //Show all transfer information
        public ActionResult Transfer05()
        {
            ViewBag.Name = Session["Name"].ToString();
            ViewBag.Number = Session["Number"].ToString();
            ViewBag.Amount = Session["Amount"].ToString();
            return View();

        }

        //Notice suscessful
        public ActionResult Transfer06()
        {
            var name = Session["Name"].ToString();
            var numberAccountTr = Session["Number"].ToString();
            var add = decimal.Parse(Session["Amount"].ToString());
            var numberAccountS = Session["AccountNoTransfer"].ToString();
            var sent = _db.Accounts.Single(ao => ao.AccountNo.Equals(numberAccountS));
            sent.Balance = sent.Balance - add;
            UpdateModel(sent);
            var old = _db.Accounts.Single(ao => ao.AccountNo.Equals(numberAccountTr));
            old.Balance = old.Balance + add;
            UpdateModel(old);
            _db.SaveChanges();
            /*Write Log Trasfer*/
            string strTemp = Session["CardNo"].ToString();
            var logT = new LogWithdrawTranfer();
            logT.WriteTransfer(strTemp, add, name);
            return View();
        }

        public ActionResult AccountNotExist()
        {
            var i = int.Parse(Session["demsolan"].ToString());
            i++;
            Session["demsolan"] = i;
            var numberEnter = 5 - i;
            ViewBag.timeInputNo = "You have only " + numberEnter + " left!";
            return View();
        }

        [HttpPost]
        public ActionResult AccountNotExist(string idRecieve)
        {
            var i = int.Parse(Session["demsolan"].ToString());

            if (i > 3) return RedirectToAction("InputCard", "Validation");
            var acRe = (from ac in _db.Accounts where ac.AccountNo.Equals(idRecieve) select ac);
            if (acRe.Any())
            {
                Session["ID"] = idRecieve;
                return RedirectToAction("Transfer03");
            }
            Session["ID"] = "";
            return RedirectToAction("AccountNotExist");
        }



        public ActionResult Receipt()
        {
            ViewBag.nameSent = Session["NameTransfer"].ToString();
            ViewBag.NoSent = Session["AccountNoTransfer"].ToString();
            ViewBag.nameRe = Session["Name"].ToString();
            ViewBag.NoRecieve = Session["Number"].ToString();
            ViewBag.Amount = Session["Amount"].ToString();
            ViewBag.Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            return View();
        }

        /*Check Acount duplicate*/
        public bool CheckDuplicate(string cardNo, string accountNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var card = dataEntities.Cards;
                var account = dataEntities.Accounts;
                var query = (from cd in card
                             join acc in account
                             on cd.AccountID equals acc.AccountID
                             where cd.CardNo.Equals(cardNo)
                             select new { acc.AccountNo }).ToList();
                if (accountNo.Equals(query[0].AccountNo))
                {
                    return false;
                }
                return true;
            }
        }



        /*Check Select the largest amount drawn during the day*/
        private bool MoneyOverTransfer(string cardNo, decimal moneyNeedTransfer)
        {
            using (var dataEntites = new ManagerATMEntities())
            {
                var tempTable = dataEntites.Logs.Where(lg => lg.CardNo.Equals(cardNo) && lg.LogTypeID.Equals(2)).ToList();

                var logTempData = (from item in tempTable
                                   let logDateSubstring = Convert.ToDateTime(item.LogDate).ToShortDateString()
                                   select new LogTemp
                                   {
                                       LogId = item.LogID,
                                       LogTypeId = item.LogTypeID,
                                       Atmid = item.ATMID,
                                       CardNo = item.CardNo,
                                       LogDate = logDateSubstring,
                                       Amount = Convert.ToDecimal(item.Amount),
                                       Detail = item.Details
                                   }).ToList();

                /*Use Linq to Object ==> Excute Interface IEnumerable*/
                var check = (from logTp in logTempData
                             where logTp.LogDate.Equals(DateTime.Now.ToShortDateString())
                             group logTp by logTp.Amount into tableTemp
                             select new { sumMoneyDayNowTemp = tableTemp.Sum(x => x.Amount) }).ToList();
                if (check.Count.Equals(0))
                {
                    return true;
                }
                var sumMoneyDayNow = Convert.ToDecimal(check[0].sumMoneyDayNowTemp);
                var moneyWithTemp = moneyNeedTransfer + sumMoneyDayNow;
                return moneyWithTemp <= 100000000;
            }
        }
    }
}
