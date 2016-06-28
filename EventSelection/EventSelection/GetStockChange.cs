using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace EventSelection
{
    class GetStockChange
    {
        /// <summary>
        /// 万德接口类实例。
        /// </summary>
        static WindAPI w = new WindAPI();
        public SortedDictionary<int, stockModifyList> changeList = new SortedDictionary<int, stockModifyList>();
        private string indexName = "000016.SH";
        public GetStockChange()
        {
            GetStockList();   
        }

        private void GetStockList()
        {
            TradeDays myTradeDays = new TradeDays(20130601, 20160626);
            int[] changeDate = new int[7] { 20130628, 20131213, 20140613, 20141212, 20150612, 20151211, 20160608 };
            w.start();
            foreach (int date in changeDate)
            {
                int today = date;
                int tomorrow = TradeDays.GetNextTradeDay(date);
                string todayStr = DateTime.ParseExact(today.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                string tomorrowStr = DateTime.ParseExact(tomorrow.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                List<string> todayList = new List<string>();
                List<string> tomorrowList = new List<string>();
                List<string> stockIn = new List<string>();
                List<string> stockOut = new List<string>();
                WindData wd = w.wset("sectorconstituent", "date=" + todayStr + ";windcode=" + indexName);
                object[] stockList = wd.data as object[];
                int num = stockList.Length / 3;
                for (int i = 0; i < num; i++)
                {
                    todayList.Add(Convert.ToString(stockList[i * 3 + 1]));
                }
                 wd = w.wset("sectorconstituent", "date=" + tomorrowStr + ";windcode=" + indexName);
                stockList = wd.data as object[];
                num = stockList.Length / 3;
                for (int i = 0; i < num; i++)
                {
                    tomorrowList.Add(Convert.ToString(stockList[i * 3 + 1]));
                }
                foreach (string code in tomorrowList)
                {
                    if (todayList.Contains(code)==false)
                    {
                        stockIn.Add(code);
                    }
                }
                foreach  (string code in todayList)
                {
                    if (tomorrowList.Contains(code)==false)
                    {
                        stockOut.Add(code);
                    }
                }
                stockModifyList myList = new stockModifyList(today, stockIn, stockOut);
                changeList.Add(today, myList);
            }
        }
    }
}
