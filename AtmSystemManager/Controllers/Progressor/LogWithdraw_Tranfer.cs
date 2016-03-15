using System;
using System.Security.Cryptography;
using System.Text;
using AtmSystemManager.Models;

namespace AtmSystemManager.Controllers.Progressor
{
    public class LogWithdrawTranfer
    {
        /* Create By: ManhNV Team Leader
         * Describe: Write history Transfer or WithDraw - Money in System Atm
         Precondition: Transfer and WithDraw to Succeed
         If fail then no recorded in the System*/

        /*Method members must create logNew first Write Log*/
        public void WriteTransfer(Log logNew)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                dataEntities.Logs.Add(logNew);
                dataEntities.SaveChanges();
            }
        }

        /* Mothod write Transfer
         * member not defind logNew, just pass the parameter
         * LogID: IDENTITY
         * LogID has value   = 2
         * ATMID has value   = get Login SystemATM
         * CardNo parameter  = get Session["CardNo"]
         * LogDate has value = get Datetime.Now
         * Amount parameter  
         * Detail: reDefined:= "Transfer: "+ amount + " into Account " + Name(on Table Customer)
         * In Project Default only used  ATMID = 1
         */
        public void WriteTransfer(string cardNo, decimal amount, string name)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
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
                dataEntities.SaveChanges();
            }
        }

        /* Mothod write WithDraw 
         * members not defind logNew, just pass the parameter
         * LogID: IDENTITY
         * LogID has value   = 2
         * ATMID has value   = get Login SystemATM
         * CardNo parameter  = get Session["CardNo"]
         * LogDate has value = get Datetime.Now
         * Amount parameter  
         * Detail: reDefined:= "WithDraw: " + amount
         * In Project current Default only used ATMID = 1
         */
        public void WriteWithDraw(string cardNo, decimal amount)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
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
                dataEntities.SaveChanges();
            }
        }

        #region Encrypt PIN and CardNo
        /*Encrypt SHA512*/
        public string EncryptionBySha512(string input)
        {
            SHA512 dEncryptionSha512 = new SHA512Managed();
            var byteHasEncryption = dEncryptionSha512.ComputeHash(Encoding.Default.GetBytes(input));
            var sbdTemp = new StringBuilder();
            foreach (byte t in byteHasEncryption)
                sbdTemp.Append(t.ToString("x2"));
            return sbdTemp.ToString();
        }

        /*Encrypt MD5*/
        private static String EncryptionByMd5(String input)
        {
            var x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            var s = new StringBuilder();
            foreach (byte b in bs) 
            { s.Append(b.ToString("x2").ToLower()); }
            return s.ToString();
        }

        /*combine 2 encryption SHA512 to repeate(2 times) and MD5(1 times)*/
        public string EncryptPin(string pin)
        {
            var encrpt1 = EncryptionBySha512(pin);
            var encrpt2 = EncryptionBySha512(encrpt1);
            return EncryptionByMd5(encrpt2);
        }
        #endregion

    }
}