using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;

namespace AtmSystemManager.Controllers
{
    public class WithdrawController : Controller
    {
        // GET: /Withdraw/

        /// <summary>
        /// Support Program and Review Code: HoangND
        /// Describe: WithDraw Money System ATM
        /// ManhNV support to HaoDT write code
        /// Create: HaoDH and ManhNV - Review Coding Conventions: ManhNV
        /// </summary>

        public ActionResult Withdraw()
        {
            if (Session["CardNo"] == null || (string) Session["CardNo"] == "")
            {
                return RedirectToAction("InputCard", "Validation");
            }
            ViewBag.err = "";
            Session["MoneyTemp"] = 0;
            return View();
        }

        [HttpPost]
        public ActionResult Withdraw(decimal amount)
        {
            if (amount.Equals(1))
            {
                return RedirectToAction("EnterOther");
            }
            Session["MoneyTemp"] = amount;
            return RedirectToAction("GetReceipt");
        }

        public ActionResult EnterOther()
        {
            ViewBag.cl = "black";
            return View();
        }

        [HttpPost]
        public ActionResult EnterOther(string amount)
        {
            /*Số tiền nhập vào không hợp lệ trả lại page cũ ==> thỏa mã trả sang page mới*/
            if (amount == "")
            {
                ViewBag.cl = "red";
                return View();
            }
            var moneyTest = Convert.ToDecimal(amount);
            if (!CheckDivisible(moneyTest))
            {
                ViewBag.cl = "red";
                return View();
            }
            Session["MoneyTemp"] = moneyTest;
            return RedirectToAction("GetReceipt");
        }

        public ActionResult GetReceipt()
        {
            Session["Error"] = "";
            Session["Error2"] = "";
            @ViewBag.Notie = "Do you want to withdraw the money";
            if (Session["MoneyTemp"] == null || Equals(Session["MoneyTemp"], ""))
            {
                return RedirectToAction("Withdraw");
            }
            var money = Session["MoneyTemp"].ToString();
            var hv = new HistoryView();
            var strConvert = hv.ChairProgress(Convert.ToDecimal(money));
            @ViewBag.Money = strConvert;
            return View();
            /**/
        }

        [HttpPost]
        public ActionResult GetReceipt(string checkbutton)
        {
            if (checkbutton.Equals("oke"))
            {
                decimal moneyTemp = Convert.ToDecimal(Session["MoneyTemp"]);
                string cardNo = Session["CardNo"].ToString();

                /*Money not enough in the account*/
                if (!CheckMoneyEnough(moneyTemp, cardNo))
                {
                    Session["Error"] = "Sorry! You do not have enough money to carry out this transaction";
                    return RedirectToAction("Error");
                }

                /*Money exceeding of the current today*/
                if (!MoneyOver(cardNo, moneyTemp))
                {
                    Session["Error"] = "Withdrawal amount exceeds the limit of the day";
                    return RedirectToAction("Error");
                }

                if (!CheckMoneyAtm(moneyTemp))
                {
                    Session["Error"] = "ATM not enough money";
                    return RedirectToAction("Error");
                }

                /*If validation 2 condition above then excute update money withdraw*/
                UpdateBalanced(cardNo, moneyTemp);
                var writeLog = new LogWithdrawTranfer();
                writeLog.WriteWithDraw(cardNo, moneyTemp);
                Session["Error2"] = "Transactions Successful -";
                return RedirectToAction("CheckTransaction");
            }
            return RedirectToAction("WithDraw");
        }

        public ActionResult Error()
        {
            @ViewBag.Err2 = Session["Error"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Error(string temp)
        {
            ViewBag.Notie2 = "";
            return RedirectToAction("CheckTransaction");
        }

        public ActionResult CheckTransaction()
        {
            if (Session["CardNo"] == null || (string) Session["CardNo"] == "")
            {
                return RedirectToAction("InputCard", "Validation");
            }
            @ViewBag.Notie2 = Session["Error2"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult CheckTransaction(string str)
        {
            return RedirectToAction("Index", "Home");
        }

        /*Update Money Balanced*/
        public void UpdateBalanced(string cardNo, decimal tempMoney)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                decimal[] value = MoneyBalance(cardNo);
                int tempAccId = Convert.ToInt32(value[2]);
                var accountNew = dataEntities.Accounts.First(acc => acc.AccountID.Equals(tempAccId));
                decimal money = accountNew.Balance - tempMoney;
                accountNew.Balance = money;
                dataEntities.SaveChanges();
            }
        }

        /*Check Money Enter must divisible 50000*/
        private bool CheckDivisible(decimal money)
        {
            if (money < 50000 || money > 5000000)
            {
                return false;
            }
            if (!(money % 50000).Equals(0))
            {
                return false;
            }
            return true;
        }

        /*Mô tả: Bảng rút tiền sẽ yêu cầu những điều kiện sau
         * Được phép rút hết và vay với khoản thấu chi
         * mỗi lần rút không được phép quá 5 Triệu
         * Mỗi ngày rút không được phép quá số tiền quy định trong WithDraw
         * Tại lựa chọn rút số tiền với giá trị khác nhau chỉ cho phép rút từ 50 nghìn trở lên và ko quá 5 Triệu
         * Sau mỗi lần rút ghi giá trị vào hệ thống
         */

        /*Select Account - 
         * Balanced
         * Value on Odraft_Limit
         * Value on WithDraw_Limit
         */

        private static void InitalData(ManagerATMEntities dataEntities, out DbSet<Card> card,
                                        out DbSet<Account> account, out DbSet<OverDraft_Limit> overdraftLimit,
                                        out DbSet<WithDraw_Limit> withdrawLimit)
        {
            card = dataEntities.Cards;
            account = dataEntities.Accounts;
            overdraftLimit = dataEntities.OverDraft_Limit;
            withdrawLimit = dataEntities.WithDraw_Limit;
        }

        /*Select the largest amount drawn during the day*/
        private static decimal MoneyOver_onWithDraw(string cardNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var card = dataEntities.Cards;
                var withdrawLimit = dataEntities.WithDraw_Limit;
                var account = dataEntities.Accounts;

                var infor = from cd in card
                            join ac in account
                            on cd.AccountID equals ac.AccountID
                            join wdl in withdrawLimit
                            on ac.WDID equals wdl.WDID
                            //join lg in logTemp
                            //on cd.CardNo equals lg.CardNo
                            where cd.CardNo.Equals(cardNo)
                            select new { ValueWD = wdl.Value };
                decimal moneyMak = 0;
                foreach (var item in infor)
                {
                    moneyMak = item.ValueWD;
                }
                return moneyMak;
            }
        }

        /*Check Select the largest amount drawn during the day*/
        private bool MoneyOver(string cardNo, decimal moneyNeedWith)
        {
            using (var dataEntites = new ManagerATMEntities())
            {
                var moneyNowMax = MoneyOver_onWithDraw(cardNo);
                var tempTable = dataEntites.Logs.Where(lg => lg.CardNo.Equals(cardNo) && lg.LogTypeID.Equals(1)).ToList();

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
                var moneyWithTemp = moneyNeedWith + sumMoneyDayNow;
                return moneyWithTemp <= moneyNowMax;
            }
        }

        /*Select Money Balanced current and Value(table Overdraft_limit)*/
        private decimal[] MoneyBalance(string cardNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                DbSet<Card> card;
                DbSet<Account> account;
                DbSet<OverDraft_Limit> overdraftLimit;
                DbSet<WithDraw_Limit> withdrawLimit;
                InitalData(dataEntities, out card, out account, out overdraftLimit, out withdrawLimit);

                var infor = (from cd in card
                             join ac in account
                             on cd.AccountID equals ac.AccountID
                             join odl in overdraftLimit
                             on ac.ODID equals odl.ODID
                             where cd.CardNo.Equals(cardNo)
                             select new { ac.Balance, odl.Value, ac.AccountID }).ToList();
                decimal[] values = { infor[0].Balance, infor[0].Value, infor[0].AccountID };
                return values;
            }
        }

        /*Accout has amount not enough - không đủ tiền*/
        private bool CheckMoneyEnough(decimal money, string cardNo)
        {
            decimal[] tempValue = MoneyBalance(cardNo);
            var moneyCurrent = tempValue[0];
            var valueLimit = tempValue[1];
            if (moneyCurrent >= 0) return true;
            var tempMoney2 = money + Math.Abs(moneyCurrent);
            return tempMoney2 <= valueLimit;
        }

        /*Money not enough ATM*/
        private bool CheckMoneyAtm(decimal moneyTemp)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var idAtm = (int) Session["ATMID"];
                var atm = dataEntities.ATMs;
                var stock = dataEntities.Stocks;
                var money = dataEntities.Moneys;
                var query = from at in atm
                            join stk in stock on at.ATMID equals stk.ATMID
                            join mn in money on stk.MoneyID equals mn.MoneyID
                            where at.ATMID.Equals(idAtm)
                            select new { mn.MoneyValue };
                var moneyTest = query.ToList()[0].MoneyValue;
                if (moneyTest < moneyTemp)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
