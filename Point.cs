using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// Represents a pair of board coordinates (x and y).
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// The X value of the coordinate.
        /// </summary>
        public int x;

        /// <summary>
        /// The Y value of the coordinate.
        /// </summary>
        public int y;

        /// <summary>
        /// Constructs a Point object from the specified coordinates.
        /// </summary>
        /// <param name="xx">The X coordinate.</param>
        /// <param name="yy">The Y coordinate.</param>
        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }

        /// <summary>
        /// Returns a string representation of the Point in the format of (x,y).
        /// </summary>
        /// <returns>Returns a string representation of the Point in the format of (x,y).</returns>
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        /// <summary>
        /// Returns a hash code based on the x and y values of the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (x << 5) + y;
        }

        /// <summary>
        /// Converts a point to SGF move format (e.g. 2,3 to "cd").
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The point in SGF format.</returns>
        public static string ConvertToSGF(int x, int y)
        {
            byte[] b = new byte[2] { (byte)(x + 97), (byte)(y + 97) };
            return ASCIIEncoding.ASCII.GetString(b);
        }

        /// <summary>
        /// Converts a point to SGF move format (e.g. 2,3 to "cd").
        /// </summary>
        /// <param name="pnt">The coordinates.</param>
        /// <returns>The point in SGF format.</returns>
        public static string ConvertToSGF(Point pnt)
        {
            if (pnt.Equals(Game.PassMove)) return "";
            return ConvertToSGF(pnt.x, pnt.y);
        }

        /// <summary>
        /// Converts an SGF format point to a Point object.
        /// </summary>
        /// <param name="sgf">The point in SGF format.</param>
        /// <returns>The Point object representing the position.</returns>
        public static Point ConvertFromSGF(string sgf)
        {
            if (sgf == "") return Game.PassMove;
            var bb = ASCIIEncoding.ASCII.GetBytes (sgf);
            int x = bb[0] >= 'a' ? bb[0] - 'a' : bb[0] - 'A' + 26;
            int y = bb[1] >= 'a' ? bb[1] - 'a' : bb[1] - 'A' + 26;
            return new Point (x, y);
        }
    }
}
