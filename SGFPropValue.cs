using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// Represents an SGF property-value, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFPropValue
    {
        /// <summary>
        /// Contains the property value.
        /// </summary>
        public string Value;

        /// <summary>
        /// Returns true if the property value is a composed value (value ':' value).
        /// </summary>
        public bool IsComposed { get { return Value.Contains(':'); } }

        /// <summary>
        /// Gets the first value of a composed value.
        /// </summary>
        public string ValX { get { return Value.Split(':')[0]; } }

        /// <summary>
        /// Gets the second value of a composed value.
        /// </summary>
        public string ValY { get { return Value.Split(':')[1]; } }

        /// <summary>
        /// Gets the first integer of a composed value.
        /// </summary>
        public int NumX { get { return int.Parse(ValX); } }

        /// <summary>
        /// Gets the second integer of a composed value.
        /// </summary>
        public int NumY { get { return int.Parse(ValY); } }

        /// <summary>
        /// Gets the property value as an integer.
        /// </summary>
        public int Num { get { return int.Parse(Value); } }

        /// <summary>
        /// Gets the property value as a real number.
        /// </summary>
        public double Double { get { return double.Parse(Value); } }

        /// <summary>
        /// Gets the property value as a move object (Point).
        /// </summary>
        public Point Move
        {
            get
            {
                return Point.ConvertFromSGF(Value);
            }
        }

        /// <summary>
        /// Gets the first move object of a composed value.
        /// </summary>
        public Point MoveA
        {
            get
            {
                return Point.ConvertFromSGF(ValX);
            }
        }

        /// <summary>
        /// Gets the second move object of a composed value.
        /// </summary>
        public Point MoveB
        {
            get
            {
                return Point.ConvertFromSGF(ValY);
            }
        }

        /// <summary>
        /// Gets the property value as a color object (Content enum).
        /// </summary>
        public Content Turn
        {
            get
            {
                return Value=="W" ? Content.White : Content.Black;
            }
        }

        /// <summary>
        /// Construct an SGFPropValue object using the specified value.
        /// </summary>
        /// <param name="v">The value of the SGFPropValue.</param>
        public SGFPropValue(string v)
        {
            Value = v;
        }

        public override string ToString ()
        {
            return Value;
        }
    }
}
