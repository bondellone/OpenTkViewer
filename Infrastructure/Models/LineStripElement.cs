using System.Collections.Generic;

namespace Infrastructure.Models
{
    public class LineStripElement<T, TK>
    {
        public readonly T Position;
        public readonly List<TK> Elements;

        public LineStripElement(T position, TK element)
        {
            Position = position;
            Elements = new List<TK>(2) { element };
        }
    }
}