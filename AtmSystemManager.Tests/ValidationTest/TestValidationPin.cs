using System;
using System.Linq;
using AtmSystemManager.Controllers;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtmSystemManager.Tests.ValidationTest
{
    [TestClass]
    public class TestValidationPin
    {
        /// <summary>
        /// Create By: ManhNV
        /// Describe: Test ValidationPIN: Valid and InValid 
        /// </summary>

        [TestMethod]
        public void UpdateWrongPinFail()
        {
            Exception exception = null;
            var controller = new ValidationController();
            try
            {
                controller.UpdateWrongPin("9704123456738");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void PinValid()
        {
            using (var dataEntites = new ManagerATMEntities())
            {

                var lgWd = new LogWithdrawTranfer();
                const string idPin = "123456";
                var pinDecrypt = lgWd.EncryptPin(idPin);
                const string temp = "9704123456788";
                var tempCardNo = lgWd.EncryptionBySha512(temp);
                var rows = dataEntites.Cards.Where(card => card.CardNo.Equals(tempCardNo));
                var rowsCheck1 = rows.Where(card => card.PIN.Equals(pinDecrypt))
                    .Select(card => new { card.CardNo }).ToList();
                Assert.AreEqual(1, rowsCheck1.Count);
            }
        }

        [TestMethod]
        public void PinInValid()
        {
            using (var dataEntites = new ManagerATMEntities())
            {

                var lgWd = new LogWithdrawTranfer();
                const string idPin = "123457";
                var pinDecrypt = lgWd.EncryptPin(idPin);
                const string temp = "9704123456781";
                var tempCardNo = lgWd.EncryptionBySha512(temp);
                var rows = dataEntites.Cards.Where(card => card.CardNo.Equals(tempCardNo));
                var rowsCheck1 = rows.Where(card => card.PIN.Equals(pinDecrypt))
                    .Select(card => new { card.CardNo }).ToList();
                Assert.AreEqual(0, rowsCheck1.Count);
            }
        }

    }
}
