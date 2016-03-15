using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;
using System;
using System.Collections.Generic;

namespace AtmSystemManager.Controllers
{
    public class ValidationController : Controller
    {
        // GET: /Validation/

        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: ManhNV - Team Leader | Review Code: ManhNV
        /// Descible: Check CardNo validate and PIN
        /// Input PIN - precondition: CardNo Valid
        /// </summary>

        public ActionResult InputCard()
        {
            ViewBag.Err = "";
            ViewBag.Err = TempData["message"];
            SetDefaultSession();
            return View();
        }

        public ActionResult InputPin()
        {
            @ViewBag.Err = "";
            return Session["CardNo"] == null || (string)Session["CardNo"] == ""
                ? (ActionResult)RedirectToAction("InputCard")
                : View();
        }

        [HttpPost]
        public ActionResult InputCard(string cardId)
        {
            /*Check Error Input*/
            using (var dataEntities = new ManagerATMEntities())
            {
                var lgWd = new LogWithdrawTranfer();
                var strCardNo = lgWd.EncryptionBySha512(cardId);
                var rowsCheck1 = dataEntities.Cards.Where(card => card.CardNo.Equals(strCardNo)).ToList();
                /*Check Account not Exist*/
                if (rowsCheck1.Count.Equals(0))
                {
                    ViewBag.Err = "Not Exist Account";
                    return View();
                }
                var rowsCheck2 = rowsCheck1.Where(card => card.ExpiredDate > DateTime.Now).ToList();

                /*Check Account Expired*/
                if (rowsCheck2.Count.Equals(0))
                {
                    ViewBag.Err = "This account has expired -Please insert the new card code";

                    /*If card Expired but not block then update status= block */
                    UpdateAttempCard(strCardNo);
                    return View();
                }
                var rowsCheck3 = rowsCheck1.Where(card => card.Status.Equals("active")).ToList();
                /*Check Account Block*/
                if (rowsCheck3.Count.Equals(0))
                {
                    ViewBag.Err = "This account has been block";
                    return View();
                }
                /*Check Attempt = 0 but Status not block*/
                var rowsCheck4 = rowsCheck1.Where(card => card.Attempt.Equals(0)).ToList();
                if (!rowsCheck4.Count.Equals(0))
                {
                    ViewBag.Err = "This account has been block";
                    UpdateAttempCard(strCardNo);
                    return View();
                }
                GetSession(rowsCheck1);
                return View("InputPin");
            }
        }

        [HttpPost]
        public ActionResult InputPin(string idPin)
        {
            if (Session["CardNo"] == null || (string)Session["CardNo"] == "")
            {
                return RedirectToAction("InputCard");
            }
            if (!idPin.Length.Equals(6))
            {
                ViewBag.Err = "PIN code must have 6 characters";
                return View();
            }
            using (var dataEntites = new ManagerATMEntities())
            {
                var lgWd = new LogWithdrawTranfer();
                var pinDecrypt = lgWd.EncryptPin(idPin);
                var tempCardNo = Session["CardNo"].ToString();
                var rows = dataEntites.Cards.Where(card => card.CardNo.Equals(tempCardNo));
                var rowsCheck1 = rows.Where(card => card.PIN.Equals(pinDecrypt))
                                     .Select(card => new { card.CardNo }).ToList();
                /*If Input wrong then update Attemp-1, vs PIN wrong and Attemp=1 ==> Overtime => Pre Page InputCard*/
                if (!rowsCheck1.Count.Equals(0)) return RedirectToAction("Index", "Home");
                var intAtempt = 0;
                foreach (var card in rows)
                {
                    intAtempt = card.Attempt;
                }
                if (!intAtempt.Equals(1))
                {
                    UpdateWrongPin(tempCardNo);
                    ViewBag.Err = "PIN code Invalid";
                    return View();
                }
                UpdateWrongPin(tempCardNo);
                if (TempData != null) TempData["message"] = "PIN code entered too many times allowed";
                SetDefaultSession();
                return RedirectToAction("InputCard");
            }
        }

        #region Method Defined

        /*Update Card-- NOT BLOCK-- and --CARD INPUT OVER 3 TIME ENTER PIN CODE--*/
        private void UpdateAttempCard(string cardNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var cardUpdate = dataEntities.Cards.First(card => card.CardNo.Equals(cardNo));
                cardUpdate.Status = "block";
                dataEntities.SaveChanges();
            }
        }

        /*Update Decrease 1 unit entering PIN Wrong*/
        public void UpdateWrongPin(string cardNo)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var cardTemp = dataEntities.Cards.Where(card => card.CardNo.Equals(cardNo))
                                                 .Select(card => new { card.Attempt });

                var intAtempOld = 0;
                foreach (var cardx in cardTemp)
                {
                    intAtempOld = cardx.Attempt;
                }

                /*If Input PIN invalid 2 time then Set Attempt=0 and Status="block"*/
                if (intAtempOld.Equals(1))
                {
                    var cardUpdate = dataEntities.Cards.First(card => card.CardNo.Equals(cardNo));
                    cardUpdate.Attempt = 0;
                    cardUpdate.Status = "block";
                    dataEntities.SaveChanges();
                }
                else
                {
                    var cardUpdate = dataEntities.Cards.First(card => card.CardNo.Equals(cardNo));
                    cardUpdate.Attempt = intAtempOld - 1;
                    dataEntities.SaveChanges();
                }
            }
        }

        /*Get Session InputCard Valid*/
        private void GetSession(IEnumerable<Card> rows)
        {
            foreach (var card in rows)
            {
                Session["CardNo"] = card.CardNo;
                Session["Status"] = card.Status;
                Session["AccountID"] = card.AccountID;
                Session["PIN"] = card.PIN;
                Session["ExpiredDate"] = card.ExpiredDate;
                Session["Attempt"] = card.Attempt;
            }
        }

        /*Set Session is empty then input Pin overtime*/
        private void SetDefaultSession()
        {
            Session["CardNo"] = "";
            Session["Status"] = "";
            Session["AccountID"] = "";
            Session["PIN"] = "";
            Session["ExpiredDate"] = "";
            Session["Attempt"] = "";
            /*In Project current Set Default ATMID = 1*/
            Session["ATMID"] = 1;
        }

        /*Check expiration date*/
        public bool CheckDate(DateTime expiredDate)
        {
            if (expiredDate < DateTime.Now)
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
