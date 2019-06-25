using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBIMApp
{
    class AxDoor
    {
        public int Id;
        public AxPolyline2d m_Polyline;
        public double m_MinZ;
        public double m_MaxZ;
        public double m_Thickness;

        public int WallId_0=-1;
        public AxSegment2 getLineSegment()
        {
            if (m_Polyline.polyline.Count<2)
            {
                return null;
            }
            Vector2d source_ = m_Polyline.polyline[0];
            Vector2d target_ = m_Polyline.polyline[m_Polyline.polyline.Count - 1];
            return new AxSegment2(source_, target_);
        }
    }
}
