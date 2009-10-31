using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public class Group
    {
        HashSet<Point> points = new HashSet<Point>(), neighbours = new HashSet<Point>();
        public Content Content { get; private set; }
        public IEnumerable<Point> Neighbours
        {
            get
            {
                return neighbours;
            }
        }
        public IEnumerable<Point> Points
        {
            get
            {
                return points;
            }
        }
        public bool IsDead { get; set; }
        public Content Territory { get; set; }

        public Group(Content c)
        {
            Content = c;
        }

        public void AddPoint(int x, int y)
        {
            points.Add(new Point(x, y));
        }

        public bool ContainsPoint(int x, int y)
        {
            return points.Contains(new Point(x, y));
        }

        public void AddNeighbour(int x, int y)
        {
            neighbours.Add(new Point(x, y));
        }
    }
}
