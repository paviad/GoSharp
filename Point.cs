using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public struct Point
    {
        public int x;
        public int y;

        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        public override int GetHashCode()
        {
            return (x << 5) + y;
        }
    }
}
