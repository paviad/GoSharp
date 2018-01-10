using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// Represents a group of stones (or empty spaces) on a board object. This
    /// object is context-free, i.e. it is not associated with a specific board.
    /// In essence it is simply a set of board coordinates, with an associated
    /// content (black, white or empty), and state (dead or alive for scoring
    /// purposes).
    /// </summary>
    public class Group
    {
        private HashSet<Point> points = new HashSet<Point>(), neighbours = new HashSet<Point>();

        /// <summary>
        /// Gets the content of the group.
        /// </summary>
        public Content Content { get; private set; }

        /// <summary>
        /// Gets an enumerator for the neighboring coordinates of the group.
        /// </summary>
        public IEnumerable<Point> Neighbours
        {
            get
            {
                return neighbours;
            }
        }

        /// <summary>
        /// Gets an enumerator for the coordinates contained in this group.
        /// </summary>
        public IEnumerable<Point> Points
        {
            get
            {
                return points;
            }
        }

        /// <summary>
        /// Gets or sets whether this group is dead for the purposes of scoring.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Gets the territory ownership color of this group of empty spaces.
        /// </summary>
        public Content Territory { get; internal set; }

        /// <summary>
        /// Constructs a group object of specified content.
        /// </summary>
        /// <param name="c">The group content.</param>
        public Group(Content c)
        {
            Content = c;
        }

        /// <summary>
        /// Adds a point to the group.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        public void AddPoint(int x, int y)
        {
            points.Add(new Point(x, y));
        }

        /// <summary>
        /// Checks whether this group contains the specified point.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns>Returns true if the point is contained in the group.</returns>
        public bool ContainsPoint(int x, int y)
        {
            return points.Contains(new Point(x, y));
        }

        /// <summary>
        /// Adds a neighbour point to the group.
        /// </summary>
        /// <param name="x">The X coordinate of the neighbour.</param>
        /// <param name="y">The Y coordinate of the neighbour.</param>
        public void AddNeighbour(int x, int y)
        {
            neighbours.Add(new Point(x, y));
        }

        /// <summary>
        /// Returns a string representation of the group as a list of points.
        /// </summary>
        /// <returns>Returns a string representation of the group as a list of points.</returns>
        public override string ToString()
        {
            if (points.Count == 0) return Content.ToString() + ":{}";
            string rc = Content.ToString() + ":{";
            foreach (var p in points) rc += p.ToString() + ",";
            rc = rc.Substring(0, rc.Length - 1) + "}";
            return rc;
        }
    }
}
