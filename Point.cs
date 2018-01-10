using System.Text;
using JetBrains.Annotations;

namespace Go {
    /// <summary>
    /// Represents a pair of board coordinates (x and y).
    /// </summary>
    public struct Point {
        /// <summary>
        /// The X value of the coordinate.
        /// </summary>
        [PublicAPI]
        public readonly int X;

        /// <summary>
        /// The Y value of the coordinate.
        /// </summary>
        [PublicAPI]
        public readonly int Y;

        /// <summary>
        /// Constructs a Point object from the specified coordinates.
        /// </summary>
        /// <param name="xx">The X coordinate.</param>
        /// <param name="yy">The Y coordinate.</param>
        public Point(int xx, int yy) {
            X = xx;
            Y = yy;
        }

        /// <summary>
        /// Returns a string representation of the Point in the format of (x,y).
        /// </summary>
        /// <returns>Returns a string representation of the Point in the format of (x,y).</returns>
        public override string ToString() {
            return "(" + X + "," + Y + ")";
        }

        /// <summary>
        /// Returns a hash code based on the x and y values of the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return (X << 5) + Y;
        }

        /// <summary>
        /// Converts a point to SGF move format (e.g. 2,3 to "cd").
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The point in SGF format.</returns>
        public static string ConvertToSGF(int x, int y) {
            // TODO: handle x and y values >26
            var b = new[] { (byte)(x + 97), (byte)(y + 97) };
            return Encoding.ASCII.GetString(b);
        }

        /// <summary>
        /// Converts a point to SGF move format (e.g. 2,3 to "cd").
        /// </summary>
        /// <param name="pnt">The coordinates.</param>
        /// <returns>The point in SGF format.</returns>
        public static string ConvertToSGF(Point pnt) {
            return pnt.Equals(Game.PassMove) ? "" : ConvertToSGF(pnt.X, pnt.Y);
        }

        /// <summary>
        /// Converts an SGF format point to a Point object.
        /// </summary>
        /// <param name="sgf">The point in SGF format.</param>
        /// <returns>The Point object representing the position.</returns>
        public static Point ConvertFromSGF(string sgf) {
            if (sgf == "") return Game.PassMove;
            var bb = Encoding.ASCII.GetBytes(sgf);
            var x = bb[0] >= 'a' ? bb[0] - 'a' : bb[0] - 'A' + 26;
            var y = bb[1] >= 'a' ? bb[1] - 'a' : bb[1] - 'A' + 26;
            return new Point(x, y);
        }
    }
}
