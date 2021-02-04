using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGatherer
{
    struct StockItem
    {
        public int key;
        public int year;
        public int month;
        public int day;
        public double open;
        public double last;
        public double high;
        public double low;
        public double volumen;

        public override string ToString()
        {
            string str = string.Concat(key.ToString(), " ", last.ToString());
            return str;
        }
    }
}
