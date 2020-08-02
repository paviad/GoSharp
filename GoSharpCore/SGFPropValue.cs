using System.Linq;

namespace GoSharpCore
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
        public bool IsComposed => Value.Contains(':');

        /// <summary>
        /// Gets the first value of a composed value.
        /// </summary>
        public string ValX => Value.Split(':')[0];

        /// <summary>
        /// Gets the second value of a composed value.
        /// </summary>
        public string ValY => Value.Split(':')[1];

        /// <summary>
        /// Gets the first integer of a composed value.
        /// </summary>
        public int NumX => int.Parse(ValX);

        /// <summary>
        /// Gets the second integer of a composed value.
        /// </summary>
        public int NumY => int.Parse(ValY);

        /// <summary>
        /// Gets the property value as an integer.
        /// </summary>
        public int Num => int.Parse(Value);

        /// <summary>
        /// Gets the property value as a real number.
        /// </summary>
        public double Double => double.Parse(Value);

        /// <summary>
        /// Gets the property value as a move object (Point).
        /// </summary>
        public Point Move => Point.ConvertFromSGF(Value);

        /// <summary>
        /// Gets the first move object of a composed value.
        /// </summary>
        public Point MoveA => Point.ConvertFromSGF(ValX);

        /// <summary>
        /// Gets the second move object of a composed value.
        /// </summary>
        public Point MoveB => Point.ConvertFromSGF(ValY);

        /// <summary>
        /// Gets the property value as a color object (Content enum).
        /// </summary>
        public Content Turn => Value=="W" ? Content.White : Content.Black;

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
