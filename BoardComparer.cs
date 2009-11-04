using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Go
{
    /// <summary>
    /// This class implements IEqualityComparer&lt;Board&gt; that compares boards by
    /// their content. The purpose of this class is to enable the super-ko rule.
    /// </summary>
    public class SuperKoComparer : IEqualityComparer<Board>
    {
        public bool Equals(Board x, Board y)
        {
            if (x.SizeX != y.SizeX || x.SizeY != y.SizeY) return false;
            for (int i = 0; i < x.SizeX; i++)
            {
                for (int j = 0; j < x.SizeY; j++)
                {
                    if (x[i, j] != y[i, j]) return false;
                }
            }
            return true;
        }

        public int GetHashCode(Board obj)
        {
            return obj.GetContentHashCode();
        }
    }
}
