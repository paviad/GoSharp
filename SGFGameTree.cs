using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    /// <summary>
    /// Represents an SGF game-tree, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFGameTree
    {
        /// <summary>
        /// Contains the SGF sequence.
        /// </summary>
        public SGFSequence Sequence = new SGFSequence();

        /// <summary>
        /// Contains a list of SGF game-tree objects.
        /// </summary>
        public List<SGFGameTree> GameTrees = new List<SGFGameTree>();

        internal void Read(TextReader sr)
        {
            char c = (char)sr.Read();
            if (c != '(')
                throw new InvalidDataException("Game-tree doesn't begin with a '('.");
            Sequence.Read(sr);
            sr.EatWS();
            while ((char)sr.Peek() == '(')
            {
                SGFGameTree gameTree = new SGFGameTree();
                gameTree.Read(sr);
                GameTrees.Add(gameTree);
                sr.EatWS();
            }
            c = (char)sr.Read();
            if (c != ')')
                throw new InvalidDataException("Game-tree doesn't end with a ')'.");
        }
    }
}
