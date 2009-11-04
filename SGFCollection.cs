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
        public List<SGFGameTree> GameTrees = new List<SGFGameTree>();

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
    }
}
