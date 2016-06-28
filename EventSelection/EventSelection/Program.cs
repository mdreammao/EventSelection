using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSelection
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetStockChange changeList = new GetStockChange();
            //GetStockData data = new GetStockData("600000.SH", 20130601, 20130630);
            GetStockBonus bonus = new GetStockBonus("600000.SH", 20130601, 20160628);
        }
    }
}
