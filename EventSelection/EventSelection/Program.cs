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
            //GetStockBonus bonus = new GetStockBonus("600000.SH", 20130601, 20160628);
            //EventOfIndex50 myEvent = new EventOfIndex50(20130601, 20130730);
            EventOfIndex50 myEvent = new EventOfIndex50(20131101, 20140130);
            myEvent = new EventOfIndex50(20140501, 20140730);
            myEvent = new EventOfIndex50(20141101, 20150130);
            myEvent = new EventOfIndex50(20150501, 20150730);
            myEvent = new EventOfIndex50(20151101, 20160130);
            myEvent = new EventOfIndex50(20160501, 20160630);

        }
    }
}
