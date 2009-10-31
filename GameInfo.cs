using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public class GameInfo
    {
        public int Handicap { get; set; }
        public double Komi { get; set; }

        public GameInfo()
        {
            Komi = 0.5;
        }
    }
}
