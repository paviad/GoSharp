using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    /// <summary>
    /// Represents a game tree complete with variations. A game tree root is identified
    /// by a non-null GameInfo property. The root of a specified game may be obtained
    /// using the Root property.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The singleton comparer for super-ko cases. There is no need for more than
        /// one instance of this.
        /// </summary>
        public static readonly SuperKoComparer SuperKoComparer = new SuperKoComparer();

        Dictionary<Point, Game> moves = new Dictionary<Point, Game>();
        Dictionary<Content, int> captures = new Dictionary<Content, int>()
        {
            { Content.Black, 0 },
            { Content.White, 0 }
        };
        HashSet<Board> superKoSet = new HashSet<Board>(SuperKoComparer);
        List<SGFProperty> sgfProperties = new List<SGFProperty>();

        /// <summary>
        /// Gets the board object of the current game position.
        /// </summary>
        public Board Board { get; private set; }

        /// <summary>
        /// Gets the color of the player whose turn it is to play.
        /// </summary>
        public Content Turn { get; private set; }

        /// <summary>
        /// Gets the GameInfo object of this game. This is null except for root
        /// Game objects.
        /// </summary>
        public GameInfo GameInfo { get; set; }

        /// <summary>
        /// Gets the coordinates of the move taken to reach this position. May be null
        /// for setup positions.
        /// </summary>
        public Point? Move { get; private set; }

        /// <summary>
        /// Gets a flag indicating whether the move used to reach this board was legal.
        /// This property may be null if this board position is the result of only setup moves.
        /// </summary>
        public bool? IsLegal { get; private set; }

        /// <summary>
        /// Gets the root Game object of the current game.
        /// </summary>
        public Game Root { get; private set; }

        /// <summary>
        /// Gets an enumerator of the move variations in this game position.
        /// </summary>
        public IEnumerable<Game> Moves
        {
            get
            {
                return moves.Values;
            }
        }

        /// <summary>
        /// Gets the number of stones white has captured
        /// (the number of black stones captured).
        /// </summary>
        public int WhiteCaptures { get { return captures[Content.White]; } }

        /// <summary>
        /// Gets the number of stones black has captured
        /// (the number of white stones captured).
        /// </summary>
        public int BlackCaptures { get { return captures[Content.Black]; } }

        /// <summary>
        /// Constructs a root game object based on a GameInfo object.
        /// </summary>
        /// <param name="gi">The GameInfo object.</param>
        public Game(GameInfo gi)
        {
            GameInfo = gi;
            InitializeFromGameInfo();
        }

        /// <summary>
        /// Constructs a root game object, along with a complete game tree from
        /// the specified SGFGameTree object.
        /// </summary>
        /// <param name="sgfGameTree">The SGF game tree object.</param>
        public Game(SGFGameTree sgfGameTree)
        {
            GameInfo = CreateGameInfoFromSGF(sgfGameTree);
            InitializeFromGameInfo();
            CreateGameTree(sgfGameTree, this);
        }

        /// <summary>
        /// Constructs a Game object from an existing Game object. This constructor is used when making
        /// game moves.
        /// </summary>
        /// <param name="fromGame">The Game object before the move.</param>
        protected Game(Game fromGame)
        {
            Board = new Board(fromGame.Board);
            Turn = fromGame.Turn.Opposite();
            captures[Content.White] = fromGame.captures[Content.White];
            captures[Content.Black] = fromGame.captures[Content.Black];
            foreach (var p in fromGame.superKoSet) superKoSet.Add(p);
            Root = fromGame.Root;
        }

        /// <summary>
        /// Constructs a Game object from a Board object and a turn to play.
        /// </summary>
        /// <param name="bs">The source Board.</param>
        /// <param name="turn">The color of the player whose turn it is to play.</param>
        protected Game(Board bs, Content turn)
        {
            Board = new Board(bs);
            Turn = turn;
        }

        private void InitializeFromGameInfo()
        {
            Board = new Board(GameInfo.BoardSizeX, GameInfo.BoardSizeY);
            if (GameInfo.Handicap > 0)
                Board.SetHandicap(GameInfo.Handicap);
            Turn = GameInfo.StartingPlayer;
            Root = this;
        }

        /// <summary>
        /// Makes a move and returns a new Game object representing the state after the
        /// move. The move is carried out whether it is legal or illegal (for example,
        /// an overwrite move). The color of the move is determined by the Turn property.
        /// The legality of the move may be determined by examining the IsLegal property
        /// of the returned object.
        /// </summary>
        /// <param name="n">The coordinates of the move.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(Point n)
        {
            return MakeMove(n.x, n.y);
        }

        /// <summary>
        /// Makes a move and returns a new Game object representing the state after the
        /// move. The move is carried out whether it is legal or illegal (for example,
        /// an overwrite move). The color of the move is determined by the Turn property.
        /// If the move was illegal (suicide, violating super-ko, or an overwrite), the
        /// method sets the legal parameter to false, otherwise it is set to true.
        /// </summary>
        /// <param name="n">The coordinates of the move.</param>
        /// <param name="legal">Set to true if the move was legal, false otherwise.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(Point n, out bool legal)
        {
            return MakeMove(n.x, n.y, out legal);
        }

        /// <summary>
        /// Makes a move and returns a new Game object representing the state after the
        /// move. The move is carried out whether it is legal or illegal (for example,
        /// an overwrite move). The color of the move is determined by the Turn property.
        /// The legality of the move may be determined by examining the IsLegal property
        /// of the returned object.
        /// </summary>
        /// <param name="x">The X coordinate of the move.</param>
        /// <param name="y">The Y coordinate of the move.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(int x, int y)
        {
            bool dummy;
            return MakeMove(x, y, out dummy);
        }

        /// <summary>
        /// Makes a move and returns a new Game object representing the state after the
        /// move. The move is carried out whether it is legal or illegal (for example,
        /// an overwrite move). The color of the move is determined by the Turn property.
        /// If the move was illegal (suicide, violating super-ko, or an overwrite), the
        /// method sets the legal parameter to false, otherwise it is set to true.
        /// </summary>
        /// <param name="x">The X coordinate of the move.</param>
        /// <param name="y">The Y coordinate of the move.</param>
        /// <param name="legal">Set to true if the move was legal, false otherwise.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(int x, int y, out bool legal)
        {
            var g = new Game(this);
            legal = g.InternalMakeMove(x, y);
            moves.Add(new Point(x, y), g);
            return g;
        }

        /// <summary>
        /// Perform the necessary operations for a move, check liberties, capture, etc. Also
        /// updates the Move and IsLegal properties.
        /// </summary>
        /// <param name="x">The X coordinate of the move.</param>
        /// <param name="y">The Y coordinate of the move.</param>
        /// <returns>True if the move was legal.</returns>
        protected bool InternalMakeMove(int x, int y)
        {
            bool legal = true;
            Content oturn = Turn.Opposite();
            if (Board[x, y] != Content.Empty) legal = false; // Overwrite move
            Board[x, y] = oturn;
            Move = new Point(x, y);
            var capturedGroups = Board.GetCapturedGroups(x, y);
            if (capturedGroups.Count == 0 && Board.GetLiberties(x, y) == 0) // Suicide move
            {
                captures[Turn] += Board.Capture(Board.GetGroupAt(x, y));
                legal = false;
            }
            else captures[oturn] += Board.Capture(capturedGroups);
            if (superKoSet != null)
            {
                if (superKoSet.Contains(Board, SuperKoComparer)) // Violates super-ko
                    legal = false;
                superKoSet.Add(Board);
            }
            IsLegal = legal;
            return legal;
        }

        /// <summary>
        /// Converts the game tree into SGF format.
        /// </summary>
        /// <param name="s">A TextWriter object that will receive the output.
        /// If null, the output is returned as a string.</param>
        /// <returns>The SGF game, or null if a TextWriter is provided.</returns>
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

        /// <summary>
        /// Parses an SGF game file and creates a list of games.
        /// </summary>
        /// <param name="path">The path to the SGF file.</param>
        /// <returns>A List&lt;Game&gt; containing all game trees in the SGF file.</returns>
        public static List<Game> SerializeFromSGF(string path)
        {
            StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII);
            SGFCollection coll = new SGFCollection();
            coll.Read(sr);
            List<Game> games = new List<Game>();
            foreach (var c in coll.GameTrees) games.Add(new Game(c));
            return games;
        }
        private static GameInfo CreateGameInfoFromSGF(SGFGameTree root)
        {
            GameInfo gi = new GameInfo();
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
                gi.BoardSizeX = sx;
                gi.BoardSizeY = sy;
            }
            var handicap = root.Sequence.Nodes[0].Properties.SingleOrDefault(x => x.Name == "HA");
            if (handicap != null)
            {
                gi.Handicap = handicap.Values[0].Num;
                if (gi.Handicap > 0)
                    gi.StartingPlayer = Content.White;
            }
            var komi = root.Sequence.Nodes[0].Properties.SingleOrDefault(x => x.Name == "KM");
            if (komi != null)
            {
                gi.Komi = komi.Values[0].Double;
            }
            return gi;
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
    }
}
