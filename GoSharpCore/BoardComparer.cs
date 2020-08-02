using System.Collections.Generic;

namespace GoSharpCore {
    /// <summary>
    /// This class implements IEqualityComparer&lt;Board&gt; that compares boards by
    /// their content. The purpose of this class is to enable the super-ko rule.
    /// </summary>
    public class SuperKoComparer : IEqualityComparer<Board> {
        /// <summary>
        /// Returns true if two Board objects have the same content.
        /// </summary>
        /// <param name="x">The first Board object.</param>
        /// <param name="y">The second Board object.</param>
        /// <returns>True if the Boards have the same content.</returns>
        public bool Equals(Board x, Board y) {
            if (x.SizeX != y.SizeX || x.SizeY != y.SizeY) {
                return false;
            }

            for (var i = 0; i < x.SizeX; i++) {
                for (var j = 0; j < x.SizeY; j++) {
                    if (x[i, j] != y[i, j]) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code based on the content of the board.
        /// </summary>
        /// <param name="obj">The Board object.</param>
        /// <returns></returns>
        public int GetHashCode(Board obj) {
            return obj.GetHashCode();
        }
    }
}
