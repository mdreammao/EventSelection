using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace EventSelection
{
    class GetStockBonus
    {
        /// <summary>
        /// 万德接口类实例。
        /// </summary>
        static private WindAPI w ;
        public List<stockChangeFormat> stockChangeList = new List<stockChangeFormat>();
        private string code, stockId, stockMarket;
        private int startDate, endDate;
        private TradeDays myTradeDays;
        public GetStockBonus(string code, int startDate, int endDate = 0)
        {
            if (endDate == 0)
            {
                endDate = startDate;
            }
            this.code = code;
            stockId = code.Substring(0, 6);
            stockMarket = code.Substring(7, 2);
            this.startDate = startDate;
            this.endDate = endDate;
            myTradeDays = new TradeDays(startDate, endDate);
            InitializeWind();
            GetBonusFromWind();
        }
        private void InitializeWind()
        {
            if (w==null)
            {
                w = new WindAPI();
                w.start();
            }
        }
        private void GetBonusFromWind()
        {
            string startDateStr = DateTime.ParseExact(startDate.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
            string endDateStr = DateTime.ParseExact(endDate.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
            WindData wd = w.wset("corporationaction", "startdate="+startDateStr+";enddate="+endDateStr+";windcode="+code);
            object[] stockList = wd.data as object[];
            int num = stockList==null?0:stockList.Length / 11;
            for (int i = 0; i < num; i++)
            {
                stockChangeFormat change = new stockChangeFormat();
                change.code = code;
                string[] date = Convert.ToString(stockList[i * 11]).Split(new char[] { '/', ' ' });
                change.date = Convert.ToInt32(date[0]) * 10000 + Convert.ToInt32(date[1]) * 100 + Convert.ToInt32(date[2]);
                change.bonus = Convert.ToDouble(stockList[i * 11 + 3]);
                change.divisor = 1.0+Convert.ToDouble(stockList[i * 11 + 4]);
                stockChangeList.Add(change);
            }
        }
    }
}
