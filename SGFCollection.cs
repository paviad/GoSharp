using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    /// <summary>
    /// Represents an SGF collection, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFCollection
    {
        /// <summary>
        /// Contains a list of SGF game-tree objects.
        /// </summary>
        public List<SGFGameTree> GameTrees = new List<SGFGameTree>();

        /// <summary>
        /// Parse an SGFCollection object from a TextReader.
        /// </summary>
        /// <param name="sr">The source TextReader.</param>
        public void Read(TextReader sr)
        {
            sr.EatWS();
            while ((char)sr.Peek() == '(')
            {
                var gameTree = new SGFGameTree();
                gameTree.Read(sr);
                GameTrees.Add(gameTree);
                sr.EatWS();
            }
        }

        /// <summary>
        /// Create an SGFCollection object from a byte array.
        /// </summary>
        /// <param name="bytes">The source byte array.</param>
        public static SGFCollection Create(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Create (ms);
            }
        }

        /// <summary>
        /// Create an SGFCollection object from a stream.
        /// </summary>
        public static SGFCollection Create (Stream stream)
        {
            var sgf = new SGFCollection ();
            using (var tr = new StreamReader (stream, Encoding.UTF8)) {
                sgf.Read (tr);
            }
            return sgf;
        }
    }
}
