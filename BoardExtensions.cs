using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public static class BoardExtensions
    {
        public static void SetHandicap(this Board board, int handicap)
        {
            List<Point> parr;
            if (handicap <= 5)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(3,15),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(8,8)
                };
            }
            else if (handicap <= 7)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(3,15),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(3,8),
                    new Point(15,8),
                    new Point(8,8)
                };
            }
            else if (handicap <= 9)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(3,15),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(3,8),
                    new Point(15,8),
                    new Point(8,3),
                    new Point(8,15),
                    new Point(8,8)
                };
            }
            else throw new InvalidCastException("Maximum handicap is 9.");
            for (int i = 0; i < handicap; i++)
                board.SetContentAt(parr[i], Content.Black);
        }
    }
}
