using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBIMApp
{
    public class AxSegment2
    {
        public Vector2d source=null;
        public Vector2d target=null;
        public AxSegment2()
        { }
        public AxSegment2(Vector2d a,Vector2d b)
        {
            source = a;
            target = b;
        }
        public Vector2d ToVector()
        {
            Vector2d tmp = target - source;
            return tmp;
        }
    }
}
