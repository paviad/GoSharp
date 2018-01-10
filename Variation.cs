namespace Go {
    internal class Variation {
        public Point Move { get; }
        public Game Game { get; }

        public Variation(Point move, Game game) {
            Move = move;
            Game = game;
        }
    }
}
