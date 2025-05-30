using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apromax
{
    public class MyDataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MyDataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
