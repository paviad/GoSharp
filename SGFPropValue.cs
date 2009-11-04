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
        public string Value;

        public bool IsComposed { get { return Value.Contains(':'); } }
        public string ValX { get { return Value.Split(':')[0]; } }
        public string ValY { get { return Value.Split(':')[1]; } }
        public int NumX { get { return int.Parse(ValX); } }
        public int NumY { get { return int.Parse(ValY); } }
        public int Num { get { return int.Parse(Value); } }
        public double Double { get { return double.Parse(Value); } }
        public Point Move
        {
            get
            {
                var bb = ASCIIEncoding.ASCII.GetBytes(Value);
                return new Point(bb[0] - 97, bb[1] - 97);
            }
        }
        public Point MoveA
        {
            get
            {
                var bb = ASCIIEncoding.ASCII.GetBytes(ValX);
                return new Point(bb[0] - 97, bb[1] - 97);
            }
        }
        public Point MoveB
        {
            get
            {
                var bb = ASCIIEncoding.ASCII.GetBytes(ValY);
                return new Point(bb[0] - 97, bb[1] - 97);
            }
        }
        public Content Turn
        {
            get
            {
                return Value=="W" ? Content.White : Content.Black;
            }
        }

        public SGFPropValue(string v)
        {
            Value = v;
        }
    }
}
