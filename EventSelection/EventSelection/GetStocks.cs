using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
using System.Data.SqlClient;
using System.Data;

namespace EventSelection
{
    /// <summary>
    /// 按照日期获取成分股列表。如果日期在调整前后，需要知道预测进出的股票。
    /// </summary>
    class GetStocks
    {
        /// <summary>
        /// 万德接口类实例。
        /// </summary>
        static WindAPI w = new WindAPI();
        /// <summary>
        /// 交易日信息的变量
        /// </summary>
        static TradeDays myTradeDays;
        /// <summary>
        /// 记录指数的成分股。
        /// </summary>
        static public Dictionary<string,stockFormat> stockList;
        /// <summary>
        /// 记录指定日期。
        /// </summary>
        public int startDate,endDate;
        /// <summary>
        /// 记录指定的指数名称。
        /// </summary>
        public string indexName;
  

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="date">指定日期</param>
        public GetStocks(int startDate,int endDate,string indexName)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.indexName = indexName;
            if (myTradeDays==null)
            {
                myTradeDays = new TradeDays(startDate, endDate);
            }
            if (stockList==null)
            {
                w.start();
                stockList = getExitsStocks();
            }
        }

        /// <summary>
        /// 根据程序运行日期，获取截止至昨日的指数成分股的列表
        /// </summary>
        /// <returns>指数成分股列表</returns>
        private Dictionary<string, stockFormat> getExitsStocks()
        {
            Dictionary<string,stockFormat> list = new Dictionary<string, stockFormat>();
            int yesterday = TradeDays.GetPreviousTradeDay(Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")));
            foreach (int day in myTradeDays.myTradeDays)
            {
                string todayStr = DateTime.ParseExact(day.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                
                if (day<=yesterday)
                {
                    
                    WindData wd = w.wset("sectorconstituent", "date="+todayStr+";windcode="+indexName);
                    object[] stockList = wd.data as object[];
                    int num = stockList.Length / 3;
                    for (int i = 0; i < num; i++)
                    {
                        stockFormat myStock = new stockFormat();
                        myStock.code =Convert.ToString(stockList[i * 3 + 1]);
                        myStock.name = (string)stockList[i * 3 + 2];
                        if (list.ContainsKey(myStock.code)==false)
                        {
                            myStock.existsDate = new List<int>();
                            myStock.existsDate.Add(day);
                            myStock.existsDate.Add(day);
                            list.Add(myStock.code, myStock);
                        }
                        else
                        {
                            int enterDate = list[myStock.code].existsDate[list[myStock.code].existsDate.Count() - 2];
                            int quitDate= list[myStock.code].existsDate[list[myStock.code].existsDate.Count() - 1];
                            if (day>TradeDays.GetNextTradeDay(quitDate))
                            {
                                list[myStock.code].existsDate.Add(day);
                                list[myStock.code].existsDate.Add(day);
                            }
                            else
                            {
                                list[myStock.code].existsDate[list[myStock.code].existsDate.Count() - 1] = day;
                            }
                        }
                      
                    }
                    
                }
            }
            //若计算时间不包括历史时间，必须读取昨日的数据作为基准
            if (list.Count==0)
            {
                string yesterdayStr = DateTime.ParseExact(yesterday.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd"); List<stockFormat> myList = new List<stockFormat>();
                WindData wd = w.wset("sectorconstituent", "date=" + yesterdayStr + ";windcode=" + indexName);
                object[] stockList = wd.data as object[];
                int num = stockList.Length / 3;
                for (int i = 0; i < num; i++)
                {
                    stockFormat myStock = new stockFormat();
                    myStock.code =Convert.ToString(stockList[i * 3 + 1]).Substring(0, 6);
                    myStock.name = (string)stockList[i * 3 + 2];
                    myStock.existsDate = new List<int>();
                    myStock.existsDate.Add(yesterday);
                    myStock.existsDate.Add(yesterday);
                    
                }
            }
            //从stockModify.csv中读取指数成分股变动股票和日期
            DataTable dt = CsvApplication.OpenCSV("stockModify.csv");
            SortedDictionary<int, List<stockModify>> stockModifyList = new SortedDictionary<int, List<stockModify>>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                stockModify stock = new stockModify();
                stock.code = dt.Rows[i][0].ToString();
                stock.name= dt.Rows[i][1].ToString();
                stock.date = Convert.ToInt32(dt.Rows[i][2].ToString());
                stock.direction= dt.Rows[i][3].ToString();
                if (stockModifyList.ContainsKey(stock.date))
                {
                    stockModifyList[stock.date].Add(stock);
                }
                else
                {
                    List<stockModify> stockList0 = new List<stockModify>();
                    stockList0.Add(stock);
                    stockModifyList.Add(stock.date, stockList0);
                }
            }
            //根据文档对我的数据进行处理，如果变动时间大于昨日时间，需要对我的股票列表进行修正
            int maxDate = yesterday;
            foreach (var stockList in stockModifyList)
            {
                if (stockList.Value[0].date>yesterday )
                {
                    foreach (var stock in stockList.Value)
                    {
                        if (stock.direction == "out")
                        {
                            if (list.ContainsKey(stock.code) == true)
                            {
                                list[stock.code].existsDate[list[stock.code].existsDate.Count() - 1] = stock.date;
                            }
                        }
                        if (stock.direction == "in")
                        {
                            if (stockList.Value[0].date>maxDate)
                            {
                                maxDate = stockList.Value[0].date;
                            }
                            if (list.ContainsKey(stock.code) == true)
                            {
                                list[stock.code].existsDate.Add(stock.date);
                                list[stock.code].existsDate.Add(stock.date);
                            }
                            else
                            {
                                stockFormat myStock = new stockFormat();
                                myStock.name = stock.name;
                                myStock.code = stock.code;
                                myStock.existsDate = new List<int>();
                                myStock.existsDate.Add(stock.date);
                                myStock.existsDate.Add(stock.date);
                            }
                        }
                    }
                }
            }

            //按照回测日期给股票列表进行修正
            foreach (var stock in list)
            {
                if (stock.Value.existsDate[stock.Value.existsDate.Count()-1]==yesterday || stock.Value.existsDate[stock.Value.existsDate.Count() - 1] == maxDate)
                {
                    stock.Value.existsDate[stock.Value.existsDate.Count() - 1] = endDate;
                }
            }
            return list;
        }

        /// <summary>
        /// 根据指定的日期，获取当日的成分股列表
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        static public List<stockFormat> getConstituentStock(int date)
        {
            List<stockFormat> list = new List<stockFormat>();
            foreach (var item in stockList)
            {
                stockFormat stock = item.Value;
                for (int i = 0; i < stock.existsDate.Count(); i=i+2)
                {
                    if (date>=stock.existsDate[i] && date<=stock.existsDate[i+1])
                    {
                        list.Add(stock);
                    }
                }
            }
            return list;
        }

        

    }
}
