using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    class SGFCollection
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
