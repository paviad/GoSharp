using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public class Board
    {
        Content[,] content;
        Group[,] groupCache2;
        List<Group> groupCache = null;
        bool _IsScoring = false;

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public bool IsScoring
        {
            get
            {
                return _IsScoring;
            }
            set
            {
                if (_IsScoring != value)
                {
                    _IsScoring = value;
                    ClearGroupCache();
                    if (value) CalcTerritory();
                }
            }
        }
        public Dictionary<Content, int> Territory
        {
            get
            {
                Dictionary<Content, int> rc = new Dictionary<Content, int>();
                int w = 0, b = 0;
                foreach (var p in groupCache.Where(x => x.Content == Content.Empty))
                {
                    if (p.Neighbours.All(x => GetContentAt(x) != Content.Black))
                    {
                        w += p.Points.Count();
                        p.Territory = Content.White;
                    }
                    else if (p.Neighbours.All(x => GetContentAt(x) != Content.White))
                    {
                        b += p.Points.Count();
                        p.Territory = Content.Black;
                    }
                    else p.Territory = Content.Empty;
                }
                foreach (var p in groupCache.Where(x => x.IsDead))
                {
                    if (p.Content == Content.Black) 
                        w += p.Points.Count() * 2;
                    else if (p.Content == Content.White) 
                        b += p.Points.Count() * 2;
                }
                rc[Content.Black] = b;
                rc[Content.White] = w;
                return rc;
            }
        }

        public Board(int sx, int sy)
        {
            content = new Content[sx, sy];
            SizeX = sx;
            SizeY = sy;
        }
        public Board(Board fromBoard)
        {
            SizeX = fromBoard.SizeX;
            SizeY = fromBoard.SizeY;
            content = new Content[SizeX, SizeY];
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    content[i, j] = fromBoard.content[i, j];
                }
            }
        }
        public Board(params int[] c)
        {
            if (c.Length == 0)
                throw new InvalidOperationException("Must provide some arguments.");
            double d = Math.Sqrt(c.Length);
            int id = (int)d;
            if (id != d)
                throw new InvalidOperationException("Argument count must be a square of a natural number.");
            SizeX = SizeY = id;
            content = new Content[id, id];
            int y = 0, x = 0;
            for (int i = 0; i < c.Length; i++)
            {
                content[x, y] = (Content)c[i];
                x++;
                if (x == SizeX)
                {
                    x = 0;
                    y++;
                }
            }
        }

        public Content this[int x, int y]
        {
            get
            {
                return GetContentAt(x, y);
            }
            set
            {
                SetContentAt(x, y, value);
            }
        }
        public Content this[Point n]
        {
            get
            {
                return GetContentAt(n.x, n.y);
            }
            set
            {
                SetContentAt(n.x, n.y, value);
            }
        }
        private Content GetContentAt(Point n)
        {
            return GetContentAt(n.x, n.y);
        }
        public Content GetContentAt(int x, int y)
        {
            if (IsScoring && content[x, y] != Content.Empty && groupCache2[x, y] != null && groupCache2[x, y].IsDead)
                return Content.Empty;
            return content[x, y];
        }
        public void SetContentAt(Point p, Content content)
        {
            SetContentAt(p.x, p.y, content);
        }
        public void SetContentAt(int x, int y, Content c)
        {
            content[x, y] = c;
            ClearGroupCache();
        }

        public Group GetGroupAt(Point n)
        {
            return GetGroupAt(n.x, n.y);
        }
        public Group GetGroupAt(int x, int y)
        {
            if (groupCache == null)
            {
                groupCache = new List<Group>();
                groupCache2 = new Group[SizeX, SizeY];
            }
            Group group = groupCache.SingleOrDefault(z => z.Points.Contains(new Point(x, y)));
            if (group == null)
            {
                group = new Group(content[x, y]);
                RecursiveAddPoint(group, x, y);
                groupCache.Add(group);
            }
            return group;
        }
        private void RecursiveAddPoint(Group group, int x, int y)
        {
            if (GetContentAt(x, y) == group.Content)
            {
                if (group.ContainsPoint(x, y)) return;
                group.AddPoint(x, y);
                groupCache2[x, y] = group;
                if (x > 0) RecursiveAddPoint(group, x - 1, y);
                if (x < SizeX - 1) RecursiveAddPoint(group, x + 1, y);
                if (y > 0) RecursiveAddPoint(group, x, y - 1);
                if (y < SizeY - 1) RecursiveAddPoint(group, x, y + 1);
            }
            else
            {
                group.AddNeighbour(x, y);
            }
        }

        public int GetLiberties(Group group)
        {
            int libs = 0;
            foreach (var n in group.Neighbours)
            {
                if (GetContentAt(n) == Content.Empty) libs++;
            }
            return libs;
        }
        public int GetLiberties(int x, int y)
        {
            return GetLiberties(GetGroupAt(x, y));
        }
        public int GetLiberties(Point n)
        {
            return GetLiberties(n.x, n.y);
        }

        public void CalcTerritory()
        {
            bool pass = true;
            while (pass)
            {
                pass = false;
                for (int i = 0; i < SizeX; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        if (groupCache2[i, j] == null)
                        {
                            GetGroupAt(i, j);
                            pass = true;
                        }
                    }
                }
            }
        }
        public void SetDeadGroup(Point n)
        {
            SetDeadGroup(n.x, n.y);
        }
        public void SetDeadGroup(int x, int y)
        {
            Group g = GetGroupAt(x, y);
            if (g.Content == Content.Empty) return;
            g.IsDead = !g.IsDead;
        }

        public List<Group> GetCapturedGroups(int x, int y)
        {
            Group group = GetGroupAt(x, y);
            List<Group> captures = new List<Group>();
            var stoneNeighbours = GetStoneNeighbours(x, y);
            foreach (var n in stoneNeighbours)
            {
                if (GetContentAt(n) != Content.Empty)
                {
                    Group ngroup = GetGroupAt(n);
                    if (GetLiberties(ngroup) == 0) captures.Add(ngroup);
                }
            }
            return captures;
        }

        private List<Point> GetStoneNeighbours(int x, int y)
        {
            List<Point> rc = new List<Point>();
            if (x > 0) rc.Add(new Point(x - 1, y));
            if (x < SizeX - 1) rc.Add(new Point(x + 1, y));
            if (y > 0) rc.Add(new Point(x, y - 1));
            if (y < SizeY - 1) rc.Add(new Point(x, y + 1));
            return rc;
        }

        public int Capture(List<Group> captures)
        {
            int rc = 0;
            foreach (var g in captures)
            {
                rc += Capture(g);
            }
            return rc;
        }
        public int Capture(Group g)
        {
            foreach (var p in g.Points)
                SetContentAt(p, Content.Empty);
            return g.Points.Count();
        }

        public void ClearGroupCache()
        {
            groupCache = null;
        }

        public int GetContentHashCode()
        {
            int hc = 0, tmp;
            foreach (var i in content)
            {
                tmp = hc >> 30;
                hc <<= 2;
                hc ^= (int)i ^ tmp;
            }
            return hc;
        }

        public override string ToString()
        {
            string rc = "";
            for (int i = 0; i < SizeY; i++)
            {
                for (int j = 0; j < SizeX; j++)
                {
                    if (content[j, i] == Content.Empty) rc += ".";
                    else if (content[j, i] == Content.Black) rc += "X";
                    else rc += "O";
                    if (IsScoring)
                    {
                        Group g = groupCache2[j, i];
                        if (g.IsDead) rc += "D";
                        else if (g.Territory == Content.Empty) rc += ".";
                        else if (g.Territory == Content.Black) rc += "x";
                        else if (g.Territory == Content.White) rc += "o";
                    }
                    rc += " ";
                }
                rc += "\n";
            }
            return rc;
        }
    }
}
