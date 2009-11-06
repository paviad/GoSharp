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
        static Dictionary<string, Content> SGFPropToColor = new Dictionary<string, Content>()
        {
            { "AE", Content.Empty },
            { "AB", Content.Black },
            { "AW", Content.White }
        };
        static Dictionary<Content, string> ColorToSGFProp = new Dictionary<Content, string>();
        static Dictionary<string, Func<Game, SGFProperty, Game>> PropertyHandlers = new Dictionary<string, Func<Game, SGFProperty, Game>>();

        static Game()
        {
            foreach (var kvp in SGFPropToColor) ColorToSGFProp[kvp.Value] = kvp.Key;

            PropertyHandlers["W"] = ((x, y) => x.HandleMove(y));
            PropertyHandlers["B"] = ((x, y) => x.HandleMove(y));
            PropertyHandlers["AE"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["AW"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["AB"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["PL"] = ((x, y) => x.HandlePlayerTurn(y));
            PropertyHandlers["HA"] = ((x, y) => x.HandleHandicap(y));
            PropertyHandlers["SZ"] = ((x, y) => x.HandleBoardSize(y));
            PropertyHandlers["KM"] = ((x, y) => x.HandleKomi(y));
        }

        Dictionary<Point, Game> moves = new Dictionary<Point, Game>();
        Dictionary<Content, int> captures = new Dictionary<Content, int>()
        {
            { Content.Black, 0 },
            { Content.White, 0 }
        };
        HashSet<Board> superKoSet = new HashSet<Board>(SuperKoComparer);
        List<SGFProperty> sgfProperties = new List<SGFProperty>();
        Dictionary<Content, Group> setupMoves = null;

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
        /// Returns a Dictionary&lt;Content, Group&gt; with the setup moves
        /// of the current node.
        /// </summary>
        public Dictionary<Content, Group> SetupMoves
        {
            get
            {
                if (setupMoves == null)
                {
                    setupMoves = new Dictionary<Content, Group>();
                    foreach (var color in SGFPropToColor)
                    {
                        setupMoves[color.Value] = new Group(color.Value);
                        foreach (var p in sgfProperties.Where(x => x.Name == color.Key))
                        {
                            Content c = color.Value;
                            Group g = setupMoves[c];
                            foreach (var v in p.Values)
                            {
                                if (v.IsComposed)
                                {
                                    for (int i = v.MoveA.x; i <= v.MoveB.x; i++)
                                    {
                                        for (int j = v.MoveA.y; j <= v.MoveB.y; j++)
                                        {
                                            g.AddPoint(i, j);
                                        }
                                    }
                                }
                                else
                                {
                                    g.AddPoint(v.Move.x, v.Move.y);
                                }
                            }
                            setupMoves[color.Value] = g;
                        }
                    }
                }
                return setupMoves;
            }
        }

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
            //GameInfo = CreateGameInfoFromSGF(sgfGameTree);
            //InitializeFromGameInfo();
            GameInfo = new GameInfo() { FreePlacedHandicap = true };
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
            if (GameInfo.Handicap > 0 && !GameInfo.FreePlacedHandicap)
                SetHandicap(GameInfo.Handicap);
            Turn = GameInfo.StartingPlayer;
            Root = this;
        }

        private void SetHandicap(int handicap)
        {
            List<Point> parr;
            if (handicap <= 5)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(3,15),
                    new Point(9,9)
                };
            }
            else if (handicap <= 7)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(3,15),
                    new Point(3,9),
                    new Point(15,9),
                    new Point(9,9)
                };
            }
            else if (handicap <= 9)
            {
                parr = new List<Point> {
                    new Point(3,3),
                    new Point(15,15),
                    new Point(15,3),
                    new Point(3,15),
                    new Point(3,9),
                    new Point(15,9),
                    new Point(9,3),
                    new Point(9,15),
                    new Point(9,9)
                };
            }
            else throw new InvalidCastException("Maximum handicap is 9.");
            for (int i = 0; i < handicap; i++)
                SetupMove(parr[i], Content.Black);
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
        /// Adds a stone to the board as a setup move.
        /// </summary>
        /// <param name="p">The coordinates of the setup move.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(Point p, Content c)
        {
            SetupMove(p.x, p.y, c);
        }

        /// <summary>
        /// Adds a stone to the board as a setup move.
        /// </summary>
        /// <param name="x">The X coordinate of the setup move.</param>
        /// <param name="y">The Y coordinate of the setup move.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(int x, int y, Content c)
        {
            SGFProperty p = sgfProperties.SingleOrDefault(z => z.Name == ColorToSGFProp[c]);
            if (p == null)
            {
                p = new SGFProperty() { Name = ColorToSGFProp[c] };
                sgfProperties.Add(p);
            }
            SGFPropValue v = new SGFPropValue(Point.ConvertToSGF(x, y));
            p.Values.Add(v);
            Board[x, y] = c;
            setupMoves = null;
        }

        /// <summary>
        /// Adds stones to the board in a rectangular area.
        /// </summary>
        /// <param name="p1">The top left coordinates of the rectangle.</param>
        /// <param name="p2">The bottom right coordinates of the rectangle.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(Point p1, Point p2, Content c)
        {
            SetupMove(p1.x, p1.y, p2.x, p2.y, c);
        }

        /// <summary>
        /// Adds stones to the board in a rectangular area.
        /// </summary>
        /// <param name="x1">The left coordinate of the rectangle.</param>
        /// <param name="y1">The top coordinate of the rectangle.</param>
        /// <param name="x2">The right coordinate of the rectangle.</param>
        /// <param name="y2">The bottom coordinate of the rectangle.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(int x1, int y1, int x2, int y2, Content c)
        {
            SGFProperty p = sgfProperties.SingleOrDefault(x => x.Name == ColorToSGFProp[c]);
            if (p == null)
            {
                p = new SGFProperty() { Name = ColorToSGFProp[c] };
                sgfProperties.Add(p);
            }
            string composed = Point.ConvertToSGF(x1, y1) + ":" + Point.ConvertToSGF(x2, y2);
            SGFPropValue v = new SGFPropValue(composed);
            p.Values.Add(v);
            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    Board[i, j] = c;
                }
            }
            setupMoves = null;
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
                s.Write("(;");
                SerializeSGFProperties(s);
                if (GameInfo.Handicap > 0) s.Write("HA[" + GameInfo.Handicap + "]");
                if (GameInfo.StartingPlayer == Content.White) s.Write("PL[W]");
                if (GameInfo.Komi != 5.5) s.Write("KM[" + GameInfo.Komi + "]");
                if (GameInfo.BoardSizeX != 19 && GameInfo.BoardSizeY != 19)
                {
                    if (GameInfo.BoardSizeX == GameInfo.BoardSizeY)
                    {
                        s.Write("SZ[" + GameInfo.BoardSizeX + "]");
                    }
                    else
                    {
                        s.Write("SZ[" + GameInfo.BoardSizeX + ":" + GameInfo.BoardSizeY + "]");
                    }
                }
            }
            else SerializeSGFProperties(s);
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
            string sgf = Point.ConvertToSGF(pnt);
            s.Write(sgf);
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
            sr.Close();
            return games;
        }
        private static void CreateGameTree(SGFGameTree root, Game p)
        {
            if (p.GameInfo != null)
            {
                foreach (var m in root.Sequence.GetRootProperties())
                {
                    if (PropertyHandlers.ContainsKey(m.Name))
                        PropertyHandlers[m.Name](p, m);
                    else
                        p.sgfProperties.Add(m);
                }
                p.InitializeFromGameInfo();
            }
            foreach (var m in root.Sequence.GetProperties())
            {
                if (PropertyHandlers.ContainsKey(m.Name))
                    p = PropertyHandlers[m.Name](p, m);
                else
                    p.sgfProperties.Add(m);
            }
            foreach (var r in root.GameTrees)
            {
                CreateGameTree(r, p);
            }
        }

        Game HandleMove(SGFProperty p)
        {
            Content c = (p.Name == "W" ? Content.White : Content.Black);
            Turn = c;
            return MakeMove(p.Values[0].Move);
        }
        Game HandleSetup(SGFProperty p)
        {
            Content c;
            if (p.Name == "AE") c = Content.Empty;
            else if (p.Name == "AW") c = Content.White;
            else c = Content.Black;
            foreach (var v in p.Values)
            {
                if (v.IsComposed)
                    SetupMove(v.MoveA, v.MoveB, c);
                else
                    SetupMove(v.Move, c);
            }
            return this;
        }
        Game HandlePlayerTurn(SGFProperty p)
        {
            if (GameInfo != null) GameInfo.StartingPlayer = p.Values[0].Turn;
            Turn = p.Values[0].Turn;
            return this;
        }
        Game HandleHandicap(SGFProperty p)
        {
            GameInfo.Handicap = p.Values[0].Num;
            return this;
        }
        Game HandleBoardSize(SGFProperty p)
        {
            SGFPropValue v = p.Values[0];
            if (v.IsComposed)
            {
                GameInfo.BoardSizeX = v.NumX;
                GameInfo.BoardSizeY = v.NumY;
            }
            else
                GameInfo.BoardSizeX = GameInfo.BoardSizeY = v.Num;
            return this;
        }
        Game HandleKomi(SGFProperty p)
        {
            GameInfo.Komi = p.Values[0].Double;
            return this;
        }
    }
}
