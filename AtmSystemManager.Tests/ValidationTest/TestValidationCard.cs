using System;
using System.Linq;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtmSystemManager.Tests.ValidationTest
{
    [TestClass]
    public class TestValidationCard
    {
        /// <summary>
        /// Create By: ManhNV
        /// Describe: Test Card Valid and Invalid
        /// </summary>


        [TestMethod]
        public void TestCardIdValid()
        {
            var dataEntities = new ManagerATMEntities();
            const string cardId = "9704123456789";
            var lgWd = new LogWithdrawTranfer();
            var strCardNo = lgWd.EncryptionBySha512(cardId);
            var rowsCheck1 = dataEntities.Cards.Where(card => card.CardNo.Equals(strCardNo)).ToList();
            Assert.AreEqual(1, rowsCheck1.Count);
        }

        [TestMethod]
        public void TestCardIdInValid()
        {
            var dataEntities = new ManagerATMEntities();
            const string cardId = "9704123456766";
            var lgWd = new LogWithdrawTranfer();
            var strCardNo = lgWd.EncryptionBySha512(cardId);
            var rowsCheck1 = dataEntities.Cards.Where(card => card.CardNo.Equals(strCardNo)).ToList();
            Assert.AreEqual(0, rowsCheck1.Count);
        }

        [TestMethod]
        public void UpdateAttempCardSuccsess()
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                const string temp = "9704123456781";
                var lgWd = new LogWithdrawTranfer();
                var cardNo = lgWd.EncryptionBySha512(temp);
                var cardUpdate = dataEntities.Cards.First(card => card.CardNo.Equals(cardNo));
                cardUpdate.Status = "block";
                var a = dataEntities.SaveChanges();
                Assert.AreEqual(0, a);
            }
        }

        [TestMethod]
        public void CardExpired()
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                const string cardId = "9704123456787";
                var lgWd = new LogWithdrawTranfer();
                var strCardNo = lgWd.EncryptionBySha512(cardId);
                var rowsCheck1 = dataEntities.Cards.Where(card => card.CardNo.Equals(strCardNo)).ToList();
                var rowsCheck2 = rowsCheck1.Where(card => card.ExpiredDate > DateTime.Now).ToList();
                var rowsTest = rowsCheck2.Count;
                Assert.AreEqual(0, rowsTest);
            }
        }

        [TestMethod]
        public void CardBlock()
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                const string cardId = "9704123456787";
                var lgWd = new LogWithdrawTranfer();
                var strCardNo = lgWd.EncryptionBySha512(cardId);
                var rowsCheck1 = dataEntities.Cards.Where(card => card.CardNo.Equals(strCardNo)).ToList();
                var rowsCheck3 = rowsCheck1.Where(card => card.Status.Equals("active")).ToList();
                var rowsTest = rowsCheck3.Count;
                Assert.AreEqual(0, rowsTest);
            }
        }
    }
}
