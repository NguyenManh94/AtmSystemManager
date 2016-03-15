using System.Linq;
using System.Web.Mvc;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;
using System.Data;

namespace AtmSystemManager.Controllers
{
    public class ChangePinController : Controller
    {
        /// <summary>
        /// Support - Review Code: HoangND - Fsoft Training Fresher
        /// Create By: DatLH - Review Codeding Convention: ManhNV
        /// </summary>

        readonly ManagerATMEntities _db = new ManagerATMEntities();
        readonly LogWithdrawTranfer _encrypt = new LogWithdrawTranfer();
        //
        // GET: /ChangePIN/

        public ActionResult Step1()
        {
            ViewBag.Error = Session["Error"];
            return View();
        }

        [HttpPost]
        public ActionResult Step1(string oldPIN)
        {
            //Session["CardNo"] = "1234567890123456";
            string cartNo = Session["CardNo"].ToString();
            string oldPinCheck = _encrypt.EncryptPin(oldPIN);
            var checkPin = _db.Cards.Where(c => c.CardNo == cartNo && c.PIN == oldPinCheck);

            if (checkPin.Count() != 1)
            {
                Session["Error"] = "Wrong PIN!";
                return RedirectToAction("Step1");
            }

            Session["Error"] = "";

            return RedirectToAction("Step2");
        }

        public ActionResult Step2()
        {
            Session["newPIN"] = "";
            ViewBag.Error = Session["Error"];
            return View();
        }

        [HttpPost]
        public ActionResult Step2(string newPIN)
        {
            if (newPIN.Length != 6)
            {
                ViewBag.Error = "PIN must 6 charactor";
                return View();
            }
            Session["newPIN"] = newPIN;
            Session["Error"] = "";
            return RedirectToAction("Step3");
        }

        public ActionResult Step3()
        {
            ViewBag.Error = Session["Error"];
            return View();
        }

        [HttpPost]
        public ActionResult Step3(string confirmPIN)
        {
            if (!Session["newPIN"].Equals(confirmPIN))
            {
                Session["Error"] = "New PIN your have just entered not match!";
                return RedirectToAction("Step2");
            }
            Session["Error"] = "";
            ChangePin(confirmPIN);
            TempData["message"] = "Chane PIN succsess - Please validate your account !";
            return RedirectToAction("InputCard", "Validation");
        }

        public ActionResult Step4()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CheckTransaction(string str)
        {
            return RedirectToAction("Index", "Home");
        }

        private void ChangePin(string newPin)
        {
            //Session["CartNo"] = "1234567890123456";
            var cartNo = Session["CardNo"].ToString();
            var encryptPin = _encrypt.EncryptPin(newPin);

            //Đổi Pin
            var card = _db.Cards.SingleOrDefault(c => c.CardNo == cartNo);
            if (card != null)
            {
                card.PIN = encryptPin;
                _db.Entry(card).State = EntityState.Modified;
            }
            _db.SaveChanges();

            #region
            ////Ghi vào Log
            //Log log = new Log();
            //log.ATMID = 1;//Mặc định
            //log.CardNo = cartNo;
            //log.LogTypeID = 4; //ID của Change PIN trong bảng LogTypes
            //log.LogDate = DateTime.Now;
            //log.Amount = 0;
            //log.Details = "Change PIN";

            //if (ModelState.IsValid)
            //{
            //    db.Logs.Add(log);
            //    db.SaveChanges();
            //}
            #endregion
        }
    }
}
