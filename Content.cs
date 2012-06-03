using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// Used to represent the content of a board position, or the color of territory.
    /// </summary>
    public enum Content
    {
        /// <summary>
        /// An empty board position, or unowned territory.
        /// </summary>
        Empty,

        /// <summary>
        /// A black board position, or black territory.
        /// </summary>
        Black,

        /// <summary>
        /// A white board position, or white territory.
        /// </summary>
        White
    }
}
