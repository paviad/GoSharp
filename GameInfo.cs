using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// Provides information used to create the root node of a game tree.
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// Gets or sets the handicap value.
        /// </summary>
        public int Handicap { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether handicap stones should be
        /// placed in the standard positions, or whether they will be manually
        /// placed using setup moves.
        /// </summary>
        public bool FreePlacedHandicap { get; set; }

        /// <summary>
        /// Gets or sets the komi value.
        /// </summary>
        public double Komi { get; set; }

        /// <summary>
        /// Gets or sets the horizontal board size.
        /// </summary>
        public int BoardSizeX { get; set; }

        /// <summary>
        /// Gets or sets the vertical board size.
        /// </summary>
        public int BoardSizeY { get; set; }

        /// <summary>
        /// Gets or sets the color of the starting player.
        /// </summary>
        public Content StartingPlayer { get; set; }

        /// <summary>
        /// Gets or sets the name of the white player.
        /// </summary>
        public string WhitePlayer { get; set; }

        /// <summary>
        /// Gets or sets the name of the black player.
        /// </summary>
        public string BlackPlayer { get; set; }

        /// <summary>
        /// Gets or sets the rank of the white player.
        /// </summary>
        public string WhiteRank { get; set; }

        /// <summary>
        /// Gets or sets the rank of the black player.
        /// </summary>
        public string BlackRank { get; set; }

        /// <summary>
        /// Gets or sets the main time of the game.
        /// </summary>
        public TimeSpan? MainTime { get; set; }

        /// <summary>
        /// Constructs a default GameInfo object, with 0 handicap, 5.5 komi 19x19 board and
        /// black as the starting player.
        /// </summary>
        public GameInfo()
        {
            Komi = 5.5;
            StartingPlayer = Content.Black;
            Handicap = 0;
            BoardSizeX = BoardSizeY = 19;
        }
    }
}
