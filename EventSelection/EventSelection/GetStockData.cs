using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace EventSelection
{
    class GetStockData
    {
        public SortedDictionary<int,stockDataFormat[]> stockData = new SortedDictionary<int, stockDataFormat[]>();
        private string code,stockId,stockMarket;
        private int startDate, endDate;
        private TradeDays myTradeDays;
        private string IP = Configuration.IP;
        private string account = Configuration.account;
        private string password = Configuration.password;
        public GetStockData(string code,int startDate,int endDate=0)
        {
            if (endDate==0)
            {
                endDate = startDate;
            }
            this.code = code;
            stockId = code.Substring(0, 6);
            stockMarket = code.Substring(7, 2);
            this.startDate = startDate;
            this.endDate = endDate;
            myTradeDays = new TradeDays(startDate, endDate);
            GetDataFromSql(IP,account,password);
        }
        
        private void GetDataFromSql(string IP,string account,string password)
        {

            foreach (int today in myTradeDays.myTradeDays)
            {
                if (stockData.ContainsKey(today))
                {
                    continue;
                }
                string todayDataBase = "TradeMarket" + (today / 100).ToString();
                string todayConnectString = "server="+IP+";database=" + todayDataBase + ";uid ="+account+";pwd="+password+";";
                string orignalConnectString = "server=" + IP + ";database=;uid =" + account + ";pwd=" + password + ";";
                string tableName= "MarketData_" + stockId + "_" + stockMarket;
                if (SqlApplication.CheckDataBaseExist(todayDataBase, orignalConnectString) == false || SqlApplication.CheckExist(todayDataBase, tableName, todayConnectString) == false)
                {
                    Console.WriteLine("There is no data of {0} in date {1}!", code, today);
                    continue;
                }
                DataApplication myData = new DataApplication(todayDataBase, todayConnectString);
                int endOfMonth = today / 100 * 100 + 31;
                DataTable dt = myData.GetDataTable(tableName);
                foreach (int now in myTradeDays.myTradeDays)
                {
                    if (now>=today && now<=endOfMonth && now<=myTradeDays.myTradeDays[myTradeDays.myTradeDays.Count()-1])
                    {
                        DataRow[] myRows = dt.Select("tdate=" + now.ToString() + " and ttime>=93000000 and ttime<=150000000");
                        int indexMax = Time2Index(150000000);
                        stockDataFormat[] myStockArray = new stockDataFormat[indexMax+1];
                        foreach (DataRow r in myRows)
                        {
                            int index = Time2Index(Convert.ToInt32(r["ttime"]));
                            myStockArray[index] = new stockDataFormat();
                            myStockArray[index].code = Convert.ToString(r["stkcd"]).Trim() + "." + stockMarket;
                            myStockArray[index].date = Convert.ToInt32(r["tdate"]);
                            myStockArray[index].time = Convert.ToInt32(r["ttime"]);
                            myStockArray[index].last = Convert.ToDouble(r["cp"]);
                            myStockArray[index].turnover = Convert.ToDouble(r["tt"]);
                            myStockArray[index].volume = Convert.ToDouble(r["ts"]);
                            myStockArray[index].ask = new double[5];
                            myStockArray[index].ask[0] = Convert.ToDouble(r["S1"]);
                            myStockArray[index].ask[1] = Convert.ToDouble(r["S2"]);
                            myStockArray[index].ask[2] = Convert.ToDouble(r["S3"]);
                            myStockArray[index].ask[3] = Convert.ToDouble(r["S4"]);
                            myStockArray[index].ask[4] = Convert.ToDouble(r["S5"]);
                            myStockArray[index].bid = new double[5];
                            myStockArray[index].bid[0] = Convert.ToDouble(r["B1"]);
                            myStockArray[index].bid[1] = Convert.ToDouble(r["B2"]);
                            myStockArray[index].bid[2] = Convert.ToDouble(r["B3"]);
                            myStockArray[index].bid[3] = Convert.ToDouble(r["B4"]);
                            myStockArray[index].bid[4] = Convert.ToDouble(r["B5"]);
                            myStockArray[index].askv = new double[5];
                            myStockArray[index].askv[0] = Convert.ToDouble(r["SV1"]);
                            myStockArray[index].askv[1] = Convert.ToDouble(r["SV2"]);
                            myStockArray[index].askv[2] = Convert.ToDouble(r["SV3"]);
                            myStockArray[index].askv[3] = Convert.ToDouble(r["SV4"]);
                            myStockArray[index].askv[4] = Convert.ToDouble(r["SV5"]);
                            myStockArray[index].bidv = new double[5];
                            myStockArray[index].bidv[0] = Convert.ToDouble(r["BV1"]);
                            myStockArray[index].bidv[1] = Convert.ToDouble(r["BV2"]);
                            myStockArray[index].bidv[2] = Convert.ToDouble(r["BV3"]);
                            myStockArray[index].bidv[3] = Convert.ToDouble(r["BV4"]);
                            myStockArray[index].bidv[4] = Convert.ToDouble(r["BV5"]);
                        }
                        stockData.Add(now, myStockArray);
                    }
                }
            }
        }
        
        private int Time2Index(int time)
        {
            int index=0;
            if (time>=93000000 && time<=113000000)
            {
                int hour = time / 10000000;
                int minute = time % 10000000 / 100000;
                int second = time % 100000 / 1000;
                index = (hour - 9) * 3600 + (minute - 30) * 60 + second;

            }
            else if (time>=130000000 &&　time<=150000000)
            {
                int hour = time / 10000000;
                int minute = time % 10000000 / 100000;
                int second = time % 100000 / 1000;
                index = (hour - 13) * 3600 + minute * 60 + second+1+7200;
            }
            return index;
        }    

    }
}
