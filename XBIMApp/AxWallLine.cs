using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBIMApp
{
   public class AxWallLine
    {
       public int WallId;
       public AxPolyline2d m_Polyline;
       public double m_MinZ;
       public double m_MaxZ;
       public double m_Thickness;

       public List<int> m_Doors=null;
       public void AddDoor(int door_id)
       {
           if (m_Doors ==null)
           {
               m_Doors = new List<int>();
           }
           m_Doors.Add(door_id);
       }
    }
}
