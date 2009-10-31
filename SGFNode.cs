using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    class SGFNode
    {
        public List<SGFProperty> Properties = new List<SGFProperty>();

        internal void Read(TextReader sr)
        {
            char c = (char)sr.Read();
            if (c != ';')
                throw new InvalidDataException("Node doesn't begin with a ';'.");
            sr.EatWS();
            while (char.IsUpper((char)sr.Peek()))
            {
                SGFProperty prop = new SGFProperty();
                prop.Read(sr);
                Properties.Add(prop);
                sr.EatWS();
            }
        }
    }
}
