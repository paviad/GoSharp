using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go {
    class Variation {
        public Point Move { get; set; }
        public Game Game { get; set; }

        public Variation(Point move, Game game) {
            Move = move;
            Game = game;
        }
    }
}
