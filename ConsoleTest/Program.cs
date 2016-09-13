using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Go;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var gi = new GameInfo();
            var g = new Game(gi);
            g.SetupMove(1, 3, Content.Black);
            g.SetupMove(1, 4, Content.Black);
            g.SetupMove(2, 2, Content.Black);
            g.SetupMove(2, 5, Content.Black);
            g.SetupMove(3, 3, Content.Black);
            g.SetupMove(3, 1, Content.Black);
            g.SetupMove(4, 1, Content.Black);
            g.SetupMove(5, 2, Content.Black);

            g.SetupMove(2, 3, Content.White);
            g.SetupMove(2, 4, Content.White);
            g.SetupMove(3, 2, Content.White);
            g.SetupMove(3, 4, Content.White);
            g.SetupMove(3, 5, Content.White);
            g.SetupMove(4, 2, Content.White);
            g.SetupMove(4, 4, Content.White);
            g.SetupMove(5, 3, Content.White);

            Console.WriteLine("{0}", g.Board);
            var result = g.MakeMove(4, 3);
            Console.WriteLine("{0}", result.Board);
        }
    }
}
