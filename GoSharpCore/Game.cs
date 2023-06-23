using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace GoSharpCore {
    /// <summary>
    /// Represents a game tree complete with variations. A game tree root is identified
    /// by a non-null GameInfo property. The root of a specified game may be obtained
    /// using the Root property.
    /// </summary>
    public class Game {
        /// <summary>
        /// The singleton comparer for super-ko cases. There is no need for more than
        /// one instance of this.
        /// </summary>
        public static readonly SuperKoComparer SuperKoComparer = new SuperKoComparer();

        private static readonly Dictionary<string, Content> SGFPropToColor = new Dictionary<string, Content>()
        {
            { "AE", Content.Empty },
            { "AB", Content.Black },
            { "AW", Content.White }
        };

        private static readonly Dictionary<Content, string> ColorToSGFProp = new Dictionary<Content, string>();

        private static readonly Dictionary<string, Func<Game, SGFProperty, Game>> PropertyHandlers =
            new Dictionary<string, Func<Game, SGFProperty, Game>>();

        private static readonly HashSet<string> PropertiesToExclude = new HashSet<string> { "W", "B", "AE", "AB", "AW" };

        static Game() {
            foreach (var kvp in SGFPropToColor) {
                ColorToSGFProp[kvp.Value] = kvp.Key;
            }

            PropertyHandlers["W"] = ((x, y) => x.HandleMove(y));
            PropertyHandlers["B"] = ((x, y) => x.HandleMove(y));
            PropertyHandlers["AE"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["AW"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["AB"] = ((x, y) => x.HandleSetup(y));
            PropertyHandlers["PL"] = ((x, y) => x.HandlePlayerTurn(y));
            PropertyHandlers["HA"] = ((x, y) => x.HandleHandicap(y));
            PropertyHandlers["SZ"] = ((x, y) => x.HandleBoardSize(y));
            PropertyHandlers["KM"] = ((x, y) => x.HandleKomi(y));
            PropertyHandlers["WP"] = ((x, y) => x.HandleWP(y));
            PropertyHandlers["BP"] = ((x, y) => x.HandleBP(y));
            PropertyHandlers["WR"] = ((x, y) => x.HandleWR(y));
            PropertyHandlers["BR"] = ((x, y) => x.HandleBR(y));
            PropertyHandlers["TM"] = ((x, y) => x.HandleTM(y));
        }

        /// <summary>
        /// Represents a 'pass' move.
        /// </summary>
        public static readonly Point PassMove = new Point(-1, -1);

        private readonly List<Variation> _moves = new List<Variation>();

        private readonly Dictionary<Content, int> _captures = new Dictionary<Content, int>()
        {
            { Content.Black, 0 },
            { Content.White, 0 }
        };

        private readonly HashSet<Board> _superKoSet = new HashSet<Board>(SuperKoComparer);
        private readonly List<SGFProperty> _sgfProperties = new List<SGFProperty>();
        private Dictionary<Content, Group> _setupMoves;

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
        [PublicAPI]
        public Dictionary<Content, Group> SetupMoves {
            get {
                if (_setupMoves != null) {
                    return _setupMoves;
                }

                _setupMoves = new Dictionary<Content, Group>();
                foreach (var color in SGFPropToColor) {
                    _setupMoves[color.Value] = new Group(color.Value);
                    foreach (var p in _sgfProperties.Where(x => x.Name == color.Key)) {
                        var c = color.Value;
                        var g = _setupMoves[c];
                        foreach (var v in p.Values) {
                            if (v.IsComposed) {
                                for (var i = v.MoveA.x; i <= v.MoveB.x; i++) {
                                    for (var j = v.MoveA.y; j <= v.MoveB.y; j++) {
                                        g.AddPoint(i, j);
                                    }
                                }
                            }
                            else {
                                g.AddPoint(v.Move.x, v.Move.y);
                            }
                        }
                        _setupMoves[color.Value] = g;
                    }
                }
                return _setupMoves;
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
        [PublicAPI]
        public IEnumerable<Game> Moves => _moves.Select(x => x.Game);

        /// <summary>
        /// Gets the number of stones white has captured
        /// (the number of black stones captured).
        /// </summary>
        [PublicAPI]
        public int WhiteCaptures => _captures[Content.White];

        /// <summary>
        /// Gets the number of stones black has captured
        /// (the number of white stones captured).
        /// </summary>
        [PublicAPI]
        public int BlackCaptures => _captures[Content.Black];

        /// <summary>
        /// Constructs a root game object based on a GameInfo object.
        /// </summary>
        /// <param name="gi">The GameInfo object.</param>
        public Game(GameInfo gi) {
            GameInfo = gi;
            InitializeFromGameInfo();
        }

        /// <summary>
        /// Constructs a root game object, along with a complete game tree from
        /// the specified SGFGameTree object.
        /// </summary>
        /// <param name="sgfGameTree">The SGF game tree object.</param>
        public Game(SGFGameTree sgfGameTree) {
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
        protected Game(Game fromGame) {
            Board = new Board(fromGame.Board);
            Turn = fromGame.Turn.Opposite();
            _captures[Content.White] = fromGame._captures[Content.White];
            _captures[Content.Black] = fromGame._captures[Content.Black];
            foreach (var p in fromGame._superKoSet) _superKoSet.Add(p);
            Root = fromGame.Root;
        }

        /// <summary>
        /// Constructs a Game object from a Board object and a turn to play.
        /// </summary>
        /// <param name="bs">The source Board.</param>
        /// <param name="turn">The color of the player whose turn it is to play.</param>
        protected Game(Board bs, Content turn) {
            Board = new Board(bs);
            Turn = turn;
        }

        private void InitializeFromGameInfo() {
            if (Root != null) {
                // Already initialized, this represents a root node with variations
                return;
            }

            Board = new Board(GameInfo.BoardSizeX, GameInfo.BoardSizeY);
            if (GameInfo.Handicap > 0 && !GameInfo.FreePlacedHandicap)
                SetHandicap(GameInfo.Handicap);
            Turn = GameInfo.StartingPlayer;
            Root = this;
        }

        private void SetHandicap(int handicap) {
            var handiPoints = new Point[9];
            var xLeft = Board.SizeX >= 13 ? 3 : 2;
            var yBottom = Board.SizeY >= 13 ? 3 : 2;
            var xMiddle = (Board.SizeX + 1) / 2 - 1;
            var yMiddle = (Board.SizeY + 1) / 2 - 1;
            var xRight = (Board.SizeX - 1 - xLeft);
            var yTop = (Board.SizeY - 1 - yBottom);
            handiPoints[0] = new Point(xLeft, yBottom);
            handiPoints[1] = new Point(xRight, yTop);
            handiPoints[2] = new Point(xRight, yBottom);
            handiPoints[3] = new Point(xLeft, yTop);
            handiPoints[4] = new Point(xLeft, yMiddle);
            handiPoints[5] = new Point(xRight, yMiddle);
            handiPoints[6] = new Point(xMiddle, yBottom);
            handiPoints[7] = new Point(xMiddle, yTop);
            handiPoints[8] = new Point(xMiddle, yMiddle);

            List<int> parr;

            if (handicap <= 5) {
                parr = new List<int> { 0, 1, 2, 3, 8 };
            }
            else if (handicap <= 7) {
                parr = new List<int> { 0, 1, 2, 3, 4, 5, 8 };
            }
            else if (handicap <= 9) {
                parr = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            }
            else {
                throw new InvalidCastException("Maximum handicap is 9.");
            }

            for (var i = 0; i < handicap; i++) {
                SetupMove(handiPoints[parr[i]], Content.Black);
            }
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
        public Game MakeMove(Point n) {
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
        /// <param name="dontAddVariationIfIllegal">Set to true to prevent the move
        /// from being added to the variations list of the current position if the move
        /// was illegal.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        [PublicAPI]
        public Game MakeMove(Point n, out bool legal, bool dontAddVariationIfIllegal = false) {
            return MakeMove(n.x, n.y, out legal, dontAddVariationIfIllegal);
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
        /// <param name="dontAddVariationIfIllegal">Set to true to prevent the move
        /// from being added to the variations list of the current position if the move
        /// was illegal.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(int x, int y, bool dontAddVariationIfIllegal = false) {
            return MakeMove(x, y, out _, dontAddVariationIfIllegal);
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
        /// <param name="dontAddVariationIfIllegal">Set to true to prevent the move
        /// from being added to the variations list of the current position if the move
        /// was illegal.</param>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game MakeMove(int x, int y, out bool legal, bool dontAddVariationIfIllegal = false) {
            var g = new Game(this);
            legal = g.InternalMakeMove(x, y);
            if (!dontAddVariationIfIllegal || legal) {
                _moves.Add(new Variation(new Point(x, y), g));
            }
            return g;
        }

        /// <summary>
        /// Makes a 'pass' move and returns a new Game object representing the state after 
        /// the move. The color of the move is determined by the Turn property.
        /// </summary>
        /// <returns>A game object representing the state of the game after the move.</returns>
        public Game Pass() {
            var g = new Game(this);
            _moves.Add(new Variation(Game.PassMove, g));
            return g;
        }

        /// <summary>
        /// Remove a variation from the current game position.
        /// </summary>
        /// <param name="variation">The game object returned from a previous call to
        /// MakeMove or Pass</param>
        /// <returns></returns>
        public bool RemoveVariation(Game variation) {
            var variationFound = _moves.Where(z => z.Game == variation).ToList();
            variationFound.ForEach(z => _moves.Remove(z));
            return variationFound.Any();
        }

        /// <summary>
        /// Adds a stone to the board as a setup move.
        /// </summary>
        /// <param name="p">The coordinates of the setup move.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(Point p, Content c) {
            SetupMove(p.x, p.y, c);
        }

        /// <summary>
        /// Adds a stone to the board as a setup move.
        /// </summary>
        /// <param name="x">The X coordinate of the setup move.</param>
        /// <param name="y">The Y coordinate of the setup move.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(int x, int y, Content c) {
            var p = _sgfProperties.SingleOrDefault(z => z.Name == ColorToSGFProp[c]);
            if (p == null) {
                p = new SGFProperty() { Name = ColorToSGFProp[c] };
                _sgfProperties.Add(p);
            }
            var v = new SGFPropValue(Point.ConvertToSGF(x, y));
            p.Values.Add(v);
            Board[x, y] = c;
            _setupMoves = null;
        }

        /// <summary>
        /// Adds stones to the board in a rectangular area.
        /// </summary>
        /// <param name="p1">The top left coordinates of the rectangle.</param>
        /// <param name="p2">The bottom right coordinates of the rectangle.</param>
        /// <param name="c">The color of the stone to add (or empty to clear).</param>
        public void SetupMove(Point p1, Point p2, Content c) {
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
        public void SetupMove(int x1, int y1, int x2, int y2, Content c) {
            var p = _sgfProperties.SingleOrDefault(x => x.Name == ColorToSGFProp[c]);
            if (p == null) {
                p = new SGFProperty() { Name = ColorToSGFProp[c] };
                _sgfProperties.Add(p);
            }
            var composed = $"{Point.ConvertToSGF(x1, y1)}:{Point.ConvertToSGF(x2, y2)}";
            var v = new SGFPropValue(composed);
            p.Values.Add(v);
            for (var i = x1; i <= x2; i++) {
                for (var j = y1; j <= y2; j++) {
                    Board[i, j] = c;
                }
            }
            _setupMoves = null;
        }

        /// <summary>
        /// Perform the necessary operations for a move, check liberties, capture, etc. Also
        /// updates the Move and IsLegal properties.
        /// </summary>
        /// <param name="x">The X coordinate of the move.</param>
        /// <param name="y">The Y coordinate of the move.</param>
        /// <returns>True if the move was legal.</returns>
        protected bool InternalMakeMove(int x, int y) {
            var legal = true;
            var oturn = Turn.Opposite();
            if (Board[x, y] != Content.Empty) {
                legal = false; // Overwrite move
            }

            Board[x, y] = oturn;
            Move = new Point(x, y);
            var capturedGroups = Board.GetCapturedGroups(x, y);
            if (capturedGroups.Count == 0 && Board.GetLiberties(x, y) == 0) // Suicide move
            {
                _captures[Turn] += Board.Capture(Board.GetGroupAt(x, y));
                legal = false;
            }
            else {
                _captures[oturn] += Board.Capture(capturedGroups.Where(p => p.Content == oturn.Opposite()));
            }

            if (_superKoSet?.Contains(Board) == true) // Violates super-ko
            {
                legal = false;
            }

            _superKoSet?.Add(Board);
            IsLegal = legal;
            return legal;
        }

        /// <summary>
        /// Converts the game tree into SGF format.
        /// </summary>
        /// <param name="s">A TextWriter object that will receive the output.
        /// If null, the output is returned as a string.</param>
        /// <returns>The SGF game, or null if a TextWriter is provided.</returns>
        public string SerializeToSGF(TextWriter s) {
            if (s == null) {
                using (s = new StringWriter()) {
                    SerializeToSGFInternal(s);
                    return s.ToString();
                }
            }

            SerializeToSGFInternal(s);
            return null;
        }

        private void SerializeToSGFInternal(TextWriter s) {
            if (GameInfo != null) {
                s.Write("(;");
                if (GameInfo.Handicap > 0) {
                    SetProperty("HA", GameInfo.Handicap.ToString(), true);
                }
                else {
                    RemoveProperty("HA");
                }

                if (GameInfo.StartingPlayer == Content.White) {
                    SetProperty("PL", "W", true);
                }
                else {
                    RemoveProperty("PL");
                }

                if (GameInfo.Komi != 5.5) {
                    SetProperty("KM", GameInfo.Komi.ToString("N2"), true);
                }
                else {
                    RemoveProperty("KM");
                }

                if (GameInfo.BoardSizeX == GameInfo.BoardSizeY) {
                    SetProperty("SZ", GameInfo.BoardSizeX.ToString(), true);
                }
                else {
                    SetProperty("SZ", $"{GameInfo.BoardSizeX}:{GameInfo.BoardSizeY}", true);
                }

                SerializeSGFProperties(s);
            }
            else SerializeSGFProperties(s);
            if (_moves.Count == 1) {
                var pnt = _moves.First().Move;
                SerializeMove(s, pnt);
                _moves.First().Game.SerializeToSGF(s);
            }
            else if (_moves.Count > 1) {
                foreach (var m in _moves) {
                    s.Write("(");
                    SerializeMove(s, m.Move);
                    m.Game.SerializeToSGF(s);
                    s.Write(")");
                }
            }
            if (GameInfo != null) {
                s.Write(")");
            }
        }

        private void RemoveProperty(string name) {
            var prop = _sgfProperties.SingleOrDefault(x => x.Name == name);
            if (prop != null) _sgfProperties.Remove(prop);
        }

        private void SetProperty(string name, string value, bool create) {
            var prop = _sgfProperties.SingleOrDefault(x => x.Name == name);
            if (prop == null && !create) return;
            if (prop == null) {
                _sgfProperties.Add(new SGFProperty {
                    Name = name,
                    Values = new List<SGFPropValue> { new SGFPropValue(value) }
                });
            }
            else
                prop.Values[0].Value = value;
        }

        private void SerializeSGFProperties(TextWriter s) {
            foreach (var p in _sgfProperties) {
                s.Write(p.Name);
                foreach (var v in p.Values) {
                    s.Write($"[{v.Value}]");
                }
            }
        }
        private void SerializeMove(TextWriter s, Point pnt) {
            s.Write(";");
            s.Write(Turn == Content.White ? "W[" : "B[");
            var sgf = Point.ConvertToSGF(pnt);
            s.Write(sgf);
            s.Write("]");
        }

        /// <summary>
        /// Parses an SGF game file and creates a list of games.
        /// </summary>
        /// <param name="path">The path to the SGF file.</param>
        /// <returns>A List&lt;Game&gt; containing all game trees in the SGF file.</returns>
        [PublicAPI]
        public static List<Game> SerializeFromSGF(string path) {
            using (var sr = new StreamReader(path, Encoding.ASCII)) {
                return SerializeFromSGF(sr);
            }
        }

        [PublicAPI]
        public static List<Game> SerializeFromSGF(TextReader sr) {
            var coll = new SGFCollection();
            coll.Read(sr);
            return coll.GameTrees.Select(c => new Game(c)).ToList();
        }

        private static void CreateGameTree(SGFGameTree root, Game p) {
            if (p.GameInfo != null) {
                foreach (var m in root.Sequence.GetRootProperties()) {
                    if (PropertyHandlers.ContainsKey(m.Name)) {
                        PropertyHandlers[m.Name](p, m);
                    }

                    if (!PropertiesToExclude.Contains(m.Name)) {
                        p._sgfProperties.Add(m);
                    }
                }
                p.InitializeFromGameInfo();
            }

            foreach (var m in root.Sequence.GetProperties()) {
                if (PropertyHandlers.ContainsKey(m.Name)) {
                    p = PropertyHandlers[m.Name](p, m);
                }

                if (!PropertiesToExclude.Contains(m.Name)) {
                    p._sgfProperties.Add(m);
                }
            }

            foreach (var r in root.GameTrees) {
                CreateGameTree(r, p);
            }
        }

        private Game HandleMove(SGFProperty p) {
            var c = (p.Name == "W" ? Content.White : Content.Black);
            Turn = c;
            return p.Values[0].Value == "" ? Pass() : MakeMove(p.Values[0].Move);
        }

        private Game HandleSetup(SGFProperty p) {
            Content c;
            switch (p.Name) {
                case "AE":
                    c = Content.Empty;
                    break;
                case "AW":
                    c = Content.White;
                    break;
                default:
                    c = Content.Black;
                    break;
            }

            foreach (var v in p.Values) {
                if (v.IsComposed) {
                    SetupMove(v.MoveA, v.MoveB, c);
                }
                else {
                    SetupMove(v.Move, c);
                }
            }

            return this;
        }

        private Game HandlePlayerTurn(SGFProperty p) {
            if (GameInfo != null) {
                GameInfo.StartingPlayer = p.Values[0].Turn;
            }

            Turn = p.Values[0].Turn;
            return this;
        }

        private Game HandleHandicap(SGFProperty p) {
            GameInfo.Handicap = p.Values[0].Num;
            return this;
        }

        private Game HandleBoardSize(SGFProperty p) {
            var v = p.Values[0];
            if (v.IsComposed) {
                GameInfo.BoardSizeX = v.NumX;
                GameInfo.BoardSizeY = v.NumY;
            }
            else {
                GameInfo.BoardSizeX = GameInfo.BoardSizeY = v.Num;
            }

            return this;
        }

        private Game HandleKomi(SGFProperty p) {
            GameInfo.Komi = p.Values[0].Double;
            return this;
        }

        private Game HandleWP(SGFProperty p) {
            GameInfo.WhitePlayer = p.Values[0].Value;
            return this;
        }

        private Game HandleBP(SGFProperty p) {
            GameInfo.BlackPlayer = p.Values[0].Value;
            return this;
        }

        private Game HandleWR(SGFProperty p) {
            GameInfo.WhiteRank = p.Values[0].Value;
            return this;
        }

        private Game HandleBR(SGFProperty p) {
            GameInfo.BlackRank = p.Values[0].Value;
            return this;
        }

        private Game HandleTM(SGFProperty p) {
            if (string.IsNullOrEmpty(p.Values[0].Value)) {
                throw new Exception("Invalid game.");
            }
            GameInfo.MainTime = TimeSpan.FromSeconds(p.Values[0].Num);
            return this;
        }
    }
}
