using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AtmSystemManager.Models;

namespace AtmSystemManager.Controllers.Progressor
{
    /// <summary>
    /// Create by: ManhNV
    /// Describe: Create HistoryView Page Historyor
    /// Has: Amount has process Char
    /// </summary>

    public class HistoryView
    {
        public string Branch { get; set; }

        public string Description { get; set; }

        public string LogDate { get; set; }

        public string Amount { get; set; }

        public string Detail { get; set; }

        /*Method processing string*/
        public string ChairProgress(decimal amount)
        {
            var tempAmout = amount.ToString(CultureInfo.InvariantCulture);
            if (amount < 100000)
            {
                return tempAmout.Substring(0, 2)
                            + "." + tempAmout.Substring(2, 3) + " -VNĐ-";
            }
            if (amount < 1000000)
            {
                return tempAmout.Substring(0, 3)
                        + "." + tempAmout.Substring(3, 3) + " -VNĐ-";
            }
            if (amount < 10000000)
            {
                return tempAmout.Substring(0, 1)
                        + "." + tempAmout.Substring(1, 3)
                        + "." + tempAmout.Substring(4, 3) + " -VNĐ-";
            }
            if (amount < 100000000)
            {
                return tempAmout.Substring(0, 2)
                        + "." + tempAmout.Substring(2, 3)
                        + "." + tempAmout.Substring(5, 3) + " -VNĐ-";
            }
			if (amount < 1000000000)
			{
				return tempAmout.Substring(0, 3)
						+ "." + tempAmout.Substring(3, 3)
						+ "." + tempAmout.Substring(6, 3) + " -VNĐ-";
			}
            return tempAmout;
        }

        /*View History Processing string*/
        public IEnumerable<HistoryView> ViewHistory(string cardNo, int day)
        {
            using (var dataEntities = new ManagerATMEntities())
            {
                var log = dataEntities.Logs;
                var logtype = dataEntities.LogTypes;
                var atm = dataEntities.ATMs;

                var date = DateTime.Now;
                var datez = date.AddDays(-day);/*Gend code datetimenow - number*/
                /*Use Create Data ... insert HistoryView*/
                var test = from lg in log
                           join lgt in logtype
                           on lg.LogTypeID equals lgt.LogTypeID
                           join at in atm
                           on lg.ATMID equals at.ATMID
                           where lg.CardNo.Equals(cardNo) && lg.LogDate >= datez
                           select new
                           {
                               BranchT = at.Branch,
                               DescriptionT = lgt.Description,
                               LogDateT = lg.LogDate.ToString(),
                               AmountT = ChairProgress(Convert.ToDecimal(lg.Amount)),
                               DetailT = lg.Details
                           };
                return test.Select(item => new HistoryView
                {
                    Branch = item.BranchT, 
                    Description = item.DescriptionT, 
                    LogDate = item.LogDateT, 
                    Amount = item.AmountT, 
                    Detail = item.DetailT
                }).ToList();
            }
        }
    }
}