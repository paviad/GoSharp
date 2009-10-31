using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    public static class TextReaderExtensions
    {
        public static void EatWS(this TextReader tr)
        {
            while (char.IsWhiteSpace((char)tr.Peek())) tr.Read();
        }
    }
}
