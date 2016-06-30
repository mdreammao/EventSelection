using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace EventSelection
{
    class EventOfIndex50
    {
        public SortedDictionary<int, stockModifyList> changeList=new SortedDictionary<int, stockModifyList>();
        public SortedDictionary<int, List<stockPosition>> positionList=new SortedDictionary<int, List<stockPosition>>();
        public SortedDictionary<int, double[]> netValue = new SortedDictionary<int, double[]>();
        public TradeDays myTradeDays;
        public EventOfIndex50(int startDate,int endDate)
        {
            GetStockChange change = new GetStockChange();
            changeList = GetStockChange.changeList;
            myTradeDays = new TradeDays(startDate, endDate);
            computePosition(startDate, endDate);
            recordNetValue();
         }

        private void recordNetValue()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("日期", Type.GetType("System.Int32"));
            dt.Columns.Add("序号", Type.GetType("System.Int32"));
            dt.Columns.Add("净值", Type.GetType("System.Double"));
            int index = 0;
            int lastDay = 0;
            string str="";
            foreach (var item in netValue)
            {
                int today = item.Key;
                double[] net = item.Value;
                if (index==0)
                {
                    str = (today / 100).ToString();
                }
                for (int i = 0; i < net.Count(); i=i+60)
                {
                    index += 1;
                    dt.Rows.Add(new object[] { today, index, net[i] });
                }
                if (lastDay!=0 && TradeDays.GetNextTradeDay(lastDay)<today)
                {
                    index = 0;
                }
                lastDay = today;
            }
            string str2 = DateTime.Now.ToString("yyMMddHH");
            CsvApplication.SaveCSV(dt, "netvalue_" +str+ "_"+str2+".csv", "new");  
        }

        private void computePosition(int startDate,int endDate)
        {
            foreach (var item in changeList)
            {
                if (item.Key<startDate || item.Key>endDate)
                {
                    continue;
                }
                SortedDictionary<string, List<stockChangeFormat>> stockChangeList = new SortedDictionary<string, List<stockChangeFormat>>();
                SortedDictionary<string, SortedDictionary<int, stockDataFormat[]>> stockDataList = new SortedDictionary<string, SortedDictionary<int, stockDataFormat[]>>();
                int date = item.Key;
                stockModifyList list = item.Value;
                List<stockPosition> stockList = new List<stockPosition>();
                int firstDate = Math.Max(TradeDays.GetNTradeDaysBefore(date, 15),startDate);
                int lastDate = Math.Min(TradeDays.GetNTradeDaysLater(date, 15),endDate);
                TradeDays myDays = new TradeDays(firstDate, lastDate);
                foreach (string stockIn in list.stockIn)
                {
                    stockPosition stock = new stockPosition();
                    stock.code = stockIn;
                    stock.position = 1;
                    stockList.Add(stock);
                }
                foreach (string stockOut in list.stockOut)
                {
                    stockPosition stock = new stockPosition();
                    stock.code = stockOut;
                    stock.position = -1;
                    stockList.Add(stock);
                }
                foreach (stockPosition stock in stockList)
                {
                    GetStockBonus bonus = new GetStockBonus(stock.code, firstDate, lastDate);
                    GetStockData data = new GetStockData(stock.code, firstDate, lastDate);
                    stockDataList.Add(stock.code, data.stockData);
                    stockChangeList.Add(stock.code, bonus.stockChangeList);
                }
                //先确定权重
                Dictionary<string, double> weight = new Dictionary<string, double>();
                foreach  (int today in myDays.myTradeDays)
                {
                    List<stockPosition> stockListToday = new List<stockPosition>();
                    foreach (stockPosition stock in stockList)
                    {
                        List<stockChangeFormat> myChange = stockChangeList[stock.code];
                        double position = stock.position;
                        double cash = stock.cash;
                        for (int i = 0; i < myChange.Count; i++)
                        {
                            if (myChange[i].date<=today && myChange[i].date>firstDate)
                            {
                                cash += position * myChange[i].bonus;
                                position *= myChange[i].divisor;
                            }
                        }
                        stockListToday.Add(new stockPosition(stock.code, today, position, cash));
                    }
                    positionList.Add(today, stockListToday);
                    //计算净值曲线
                    double[] todayNetValue =new double[14402];//一天分成14402秒
                    //先计算权重
                    if (today==firstDate)
                    {
                        double stockInNum = 0;
                        double stockOutNum = 0;
                        Dictionary<string, double> stockOpen = new Dictionary<string, double>();
                        foreach (stockPosition myStock in stockListToday)
                        {
                            if (stockDataList.ContainsKey(myStock.code) && stockDataList[myStock.code].ContainsKey(today))
                            {
                                double[] data = dataModify(stockDataList[myStock.code][today]);
                                stockOpen.Add(myStock.code, data[0]);
                            }
                            else
                            {
                                stockOpen.Add(myStock.code, 0);
                            }   
                        }
                        foreach (stockPosition myStock in stockListToday)
                        {
                            if (myStock.position>0 && stockOpen[myStock.code]>0)
                            {
                                stockInNum += 1;
                            }
                            if (myStock.position<0 && stockOpen[myStock.code]>0)
                            {
                                stockOutNum += 1;
                            }
                        }
                        foreach (stockPosition myStock in stockListToday)
                        {
                            if (myStock.position > 0 && stockOpen[myStock.code] > 0)
                            {
                                weight.Add(myStock.code, 1.0 / stockInNum / stockOpen[myStock.code]);
                            }
                            if (myStock.position < 0 && stockOpen[myStock.code] > 0)
                            {
                                weight.Add(myStock.code, 1.0 / stockOutNum / stockOpen[myStock.code]);
                            }
                        }
                    }
                    foreach (stockPosition myStock in stockListToday)
                    {
                        if (weight.ContainsKey(myStock.code)==false)
                        {
                            continue;
                        }
                        stockDataFormat[] myStockData = (stockDataList.ContainsKey(myStock.code) && stockDataList[myStock.code].ContainsKey(today))? stockDataList[myStock.code][today]:new stockDataFormat[14402];
                        double[] data = dataModify(myStockData);
                        for (int i = 0; i < 14402; i++)
                        {
                            todayNetValue[i] += (myStock.position * data[i] + myStock.cash)*weight[myStock.code];
                        }
                    }
                    netValue.Add(today, todayNetValue);
                }
            }
        }
        
        private double[] dataModify(stockDataFormat[] myStockData)
        {
            double[] data = new double[14402];
            data[0] = myStockData[0].last;
            if (myStockData[0].last==0)
            {
                for (int i = 1; i < myStockData.Count(); i++)
                {
                    if (myStockData[i].last>0)
                    {
                        data[0] = myStockData[i].last;
                        break;
                    }
                }
            }
            for (int i = 1; i < myStockData.Count(); i++)
            {
                if (myStockData[i].last==0)
                {
                    data[i] = data[i - 1];
                }
                else
                {
                    data[i] = myStockData[i].last;
                }
            }
            return data;
        }
    }
}
