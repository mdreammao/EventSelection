using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSelection
{
    struct stockModifyList
    {
        public int date;
        public List<string> stockIn;
        public List<string> stockOut;
        public stockModifyList(int date,List<string> stockIn,List<string> stockOut)
        {
            this.date = date;
            this.stockIn = new List<string>();
            this.stockOut = new List<string>();
            foreach (string code in stockIn)
            {
                this.stockIn.Add(code);
            }
            foreach (string code in stockOut)
            {
                this.stockOut.Add(code);
            }
        }
    }

    struct stockDataFormat
    {
        public string code;
        public int date, time;
        public double last, volume, turnover;
        public double[] ask, bid, askv, bidv;
    }
    

    /// <summary>
    /// 存储股票基本信息结构体。
    /// </summary>
    struct stockFormat
    {
        public string name;
        public string code;
        //记录股票加入指数的时间，和退出的时间。。
        public List<int> existsDate;
    }

    struct stockChangeFormat
    {
        public string code;
        public int date;
        public double bonus, divisor;
    }

    /// <summary>
    /// 记录成分股变动的结构
    /// </summary>
    struct stockModify
    {
        public string name;
        public string code;
        public int date;
        public string direction;
    }
}
