using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    public class Game
    {
        public static readonly SuperKoComparer SuperKoComparer = new SuperKoComparer();

        Dictionary<Point, Game> moves = new Dictionary<Point, Game>();
        Dictionary<Content, int> captures = new Dictionary<Content, int>();
        HashSet<Board> superKoSet = new HashSet<Board>(SuperKoComparer);
        List<SGFProperty> sgfProperties = new List<SGFProperty>();

        public Board Board { get; private set; }
        public Content Turn { get; private set; }
        public GameInfo GameInfo { get; set; }
        public IEnumerable<Game> Moves
        {
            get
            {
                return moves.Values;
            }
        }
        public int WhiteCaptures { get { return captures[Content.White]; } }
        public int BlackCaptures { get { return captures[Content.Black]; } }

        public Game(Game fromGame)
        {
            Board = new Board(fromGame.Board);
            Turn = fromGame.Turn.Opposite();
            captures.Add(Content.White, fromGame.captures[Content.White]);
            captures.Add(Content.Black, fromGame.captures[Content.Black]);
            foreach (var p in fromGame.superKoSet) superKoSet.Add(p);
        }
        public Game(Board bs, Content turn)
        {
            Board = new Board(bs);
            Turn = turn;
            captures.Add(Content.Black, 0);
            captures.Add(Content.White, 0);
        }

        public Game MakeMove(Point n)
        {
            return MakeMove(n.x, n.y);
        }
        public Game MakeMove(int x, int y)
        {
            var g = new Game(this);
            if (!g.InternalMakeMove(x, y)) return null;
            moves.Add(new Point(x, y), g);
            return g;
        }

        private bool InternalMakeMove(int x, int y)
        {
            Content oturn = Turn.Opposite();
            Board[x, y] = oturn;
            var capturedGroups = Board.GetCapturedGroups(x, y);
            if (capturedGroups.Count == 0 && Board.GetLiberties(x, y) == 0) return false;
            captures[oturn] += Board.Capture(capturedGroups);
            if (superKoSet != null)
            {
                if (superKoSet.Contains(Board, SuperKoComparer)) return false;
                superKoSet.Add(Board);
            }
            return true;
        }

        public string SerializeToSGF(TextWriter s)
        {
            bool top = false;
            StringWriter sw = null;
            if (s == null)
            {
                s = sw = new StringWriter();
                top = true;
            }
            if (GameInfo != null)
            {
                s.Write("(;FF[4]");
                if (GameInfo.Handicap > 0) s.Write(";HA[" + GameInfo.Handicap + "]");
            }
            SerializeSGFProperties(s);
            if (moves.Count == 1)
            {
                Point pnt = moves.First().Key;
                SerializeMove(s, pnt);
                moves.First().Value.SerializeToSGF(s);
            }
            else if (moves.Count > 1)
            {
                foreach (var m in moves)
                {
                    s.Write("(");
                    SerializeMove(s, m.Key);
                    m.Value.SerializeToSGF(s);
                    s.Write(")");
                }
            }
            if (GameInfo != null)
            {
                s.Write(")");
            }
            if (top)
            {
                sw.Close();
                return sw.ToString();
            }
            else return null;
        }
        private void SerializeSGFProperties(TextWriter s)
        {
            foreach (var p in sgfProperties)
            {
                s.Write(p.Name);
                foreach (var v in p.Values)
                {
                    s.Write("[" + v.Value + "]");
                }
            }
        }
        private void SerializeMove(TextWriter s, Point pnt)
        {
            s.Write(";");
            s.Write(Turn == Content.White ? "W[" : "B[");
            byte[] b = new byte[2] { (byte)(pnt.x + 97), (byte)(pnt.y + 97) };
            s.Write(ASCIIEncoding.ASCII.GetString(b));
            s.Write("]");
        }

        public static List<Game> SerializeFromSGF(string path)
        {
            StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII);
            SGFCollection coll = new SGFCollection();
            coll.Read(sr);
            List<Game> games = new List<Game>();
            foreach (var c in coll.GameTrees) games.Add(CreateGame(c));
            return games;
        }
        private static Game CreateGame(SGFGameTree root)
        {
            GameInfo gi = new GameInfo();
            Game g;
            var size = root.Sequence.Nodes[0].Properties.SingleOrDefault(x => x.Name == "SZ");
            if (size != null)
            {
                int sx, sy;
                if (size.Values[0].IsComposed)
                {
                    sx = size.Values[0].NumX;
                    sy = size.Values[0].NumY;
                }
                else sy = sx = size.Values[0].Num;
                g = new Game(new Board(sx, sy), Content.Black);
            }
            else g = new Game(new Board(19, 19), Content.Black);
            var handicap = root.Sequence.Nodes[0].Properties.SingleOrDefault(x => x.Name == "HA");
            if (handicap != null)
            {
                gi.Handicap = handicap.Values[0].Num;
                if (gi.Handicap > 0)
                {
                    g.Turn = Content.White;
                    g.Board.SetHandicap(gi.Handicap);
                }
            }
            var komi = root.Sequence.Nodes[0].Properties.SingleOrDefault(x => x.Name == "KM");
            if (komi != null)
            {
                gi.Komi = komi.Values[0].Double;
            }
            g.GameInfo = gi;
            CreateGameTree(root, g);
            return g;
        }
        private static void CreateGameTree(SGFGameTree root, Game p)
        {
            foreach (var m in root.Sequence.GetMoves())
            {
                if (m.IsMove)
                    p = p.MakeMove(m.Values[0].Move);
                else if (m.IsSetup)
                {
                    Content c = Content.Empty;
                    if (m.Name == "AB") c = Content.Black;
                    else if (m.Name == "AW") c = Content.White;
                    else if (m.Name == "AE") c = Content.Empty;
                    else if (m.Name == "PL") p.Turn = m.Values[0].Turn;

                    if (m.Name != "PL")
                    {
                        foreach (var v in m.Values)
                        {
                            if (v.IsComposed)
                            {
                                Point a = v.MoveA, b = v.MoveB;
                                for (int i = a.x; i <= b.x; i++)
                                {
                                    for (int j = a.y; j <= b.y; j++)
                                        p.Board[i, j] = c;
                                }
                            }
                            else
                                p.Board[v.Move] = c;
                        }
                    }
                    p.sgfProperties.Add(m);
                }
                else
                {
                    if (!m.IsFileFormat)
                        p.sgfProperties.Add(m);
                }
            }
            foreach (var r in root.GameTrees)
            {
                CreateGameTree(r, p);
            }
        }

        public static void Test()
        {
            string s = @"
(;FF[4]AW[aa:dd]C[Lalala];B[ae]C[hfosdhfsf])
";
            StringReader sr = new StringReader(s);
            SGFCollection coll = new SGFCollection();
            coll.Read(sr);
            Game g = CreateGame(coll.GameTrees[0]);
            string r = g.SerializeToSGF(null);
            Console.WriteLine(g.GameInfo.Handicap);
            Console.WriteLine(r);
        }
    }
}
