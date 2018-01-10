using System.Collections.Generic;
using System.Linq;
using System.IO;
using JetBrains.Annotations;

namespace Go {
    /// <summary>
    /// Represents an SGF sequence, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFSequence {
        /// <summary>
        /// Contains a list of SGF nodes.
        /// </summary>
        [PublicAPI]
        public List<SGFNode> Nodes = new List<SGFNode>();

        internal void Read(TextReader sr) {
            sr.EatWs();
            while ((char)sr.Peek() == ';') {
                var node = new SGFNode();
                node.Read(sr);
                Nodes.Add(node);
                sr.EatWs();
            }
        }

        internal IEnumerable<SGFProperty> GetAllProperties() {
            return Nodes.SelectMany(x => x.Properties);
        }

        internal IEnumerable<SGFProperty> GetProperties() {
            return Nodes.SelectMany(x => x.Properties.Where(y => !y.IsRoot).OrderBy(y => y.Priority));
        }

        internal IEnumerable<SGFProperty> GetRootProperties() {
            return Nodes.SelectMany(x => x.Properties.Where(y => y.IsRoot));
        }
    }
}
