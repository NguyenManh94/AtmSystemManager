using System;
using AtmSystemManager.Controllers.Progressor;
using AtmSystemManager.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtmSystemManager.Tests.LogTest
{
    [TestClass]
    public class Logtest
    {
        /// <summary>
        /// Describe: ManhNV - Test Log
        /// </summary>

        [TestMethod]
        public void TestLogTransferSuccess()
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                const string cardId = "9704123456787";
                var lgWd = new LogWithdrawTranfer();
                var cardNo = lgWd.EncryptionBySha512(cardId);
                const int amount = 5000000;
                const string name = "NguyenManh";
                var logNew = new Log
                {
                    LogTypeID = 2,
                    ATMID = 1,
                    CardNo = cardNo,
                    LogDate = DateTime.Now,
                    Amount = amount,
                    Details = "Transfer: " + amount + " into Account: " + name
                };
                dataEntities.Logs.Add(logNew);
                var test = dataEntities.SaveChanges();
                Assert.AreEqual(1, test);
            }
        }

        [TestMethod]
        public void TestLogWithdrawSuccess()
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                const string cardId = "9704123456787";
                var lgWd = new LogWithdrawTranfer();
                var cardNo = lgWd.EncryptionBySha512(cardId);
                const int amount = 5000000;
                var logNew = new Log
                {
                    LogTypeID = 1,
                    ATMID = 1,
                    CardNo = cardNo,
                    LogDate = DateTime.Now,
                    Amount = amount,
                    Details = "WithDraw: " + amount
                };
                dataEntities.Logs.Add(logNew);
                var test = dataEntities.SaveChanges();
                Assert.AreEqual(1, test);
            }
        }
    }
}
