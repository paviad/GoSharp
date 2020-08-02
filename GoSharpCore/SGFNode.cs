using System.Collections.Generic;
using System.IO;

namespace GoSharpCore {
    /// <summary>
    /// Represents an SGF node, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFNode {
        /// <summary>
        /// Contains a list of SGF properties.
        /// </summary>
        public List<SGFProperty> Properties = new List<SGFProperty>();

        internal void Read(TextReader sr) {
            var c = (char)sr.Read();
            if (c != ';') {
                throw new InvalidDataException("Node doesn't begin with a ';'.");
            }

            sr.EatWS();
            while (char.IsUpper((char)sr.Peek())) {
                var prop = new SGFProperty();
                prop.Read(sr);
                Properties.Add(prop);
                sr.EatWS();
            }
        }
    }
}
