using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    public static class ContentExtensions
    {
        public static Content Opposite(this Content c)
        {
            return c == Content.Black ? Content.White : Content.Black;
        }
    }
}
