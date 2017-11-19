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

            g.SetupMove(1, 0, Content.Black);
            g.SetupMove(1, 1, Content.Black);
            g.SetupMove(1, 2, Content.Black);
            g.SetupMove(2, 3, Content.Black);
            g.SetupMove(3, 0, Content.Black);
            g.SetupMove(3, 3, Content.Black);
            g.SetupMove(4, 2, Content.Black);
            g.SetupMove(5, 2, Content.Black);
            g.SetupMove(6, 2, Content.Black);
            g.SetupMove(7, 0, Content.Black);
            g.SetupMove(7, 1, Content.Black);
            g.SetupMove(7, 2, Content.Black);

            g.SetupMove(2, 0, Content.White);
            g.SetupMove(2, 1, Content.White);
            g.SetupMove(2, 2, Content.White);
            g.SetupMove(3, 2, Content.White);
            g.SetupMove(4, 0, Content.White);
            g.SetupMove(4, 1, Content.White);
            g.SetupMove(5, 0, Content.White);
            g.SetupMove(5, 1, Content.White);
            g.SetupMove(6, 0, Content.White);
            g.SetupMove(6, 1, Content.White);


            Console.WriteLine("{0}", g.Board);
            var result = g.MakeMove(3, 1);
            Console.WriteLine("{0}", result.Board);
        }
    }
}
