using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    class SGFSequence
    {
        public List<SGFNode> Nodes = new List<SGFNode>();

        internal void Read(TextReader sr)
        {
            sr.EatWS();
            while ((char)sr.Peek() == ';')
            {
                SGFNode node = new SGFNode();
                node.Read(sr);
                Nodes.Add(node);
                sr.EatWS();
            }
        }

        internal IEnumerable<SGFProperty> GetMoves()
        {
            foreach (var n in Nodes)
            {
                foreach (var p in n.Properties.OrderBy(x => x.Priority))
                {
                    yield return p;
                }
            }
        }
    }
}
