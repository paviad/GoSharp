using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    class SGFGameTree
    {
        public SGFSequence Sequence = new SGFSequence();
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
