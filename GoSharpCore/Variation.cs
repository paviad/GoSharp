namespace GoSharpCore {
    internal class Variation {
        public Point Move { get; set; }
        public Game Game { get; set; }

        public Variation(Point move, Game game) {
            Move = move;
            Game = game;
        }
    }
}
