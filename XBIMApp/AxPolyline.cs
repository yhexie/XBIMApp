using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBIMApp
{
   public class AxPolyline
    {
       public List<double> polylineX;
       public List<double> polylineY;
        public AxPolyline()
        {
            polylineX = new List<double>();
            polylineY = new List<double>();
        }
    }
   public class AxPolyline2d
   {
       public List<Vector2d> polyline;

       public AxPolyline2d()
       {
           polyline = new List<Vector2d>();
       }

       public Vector2d ToVector()
       {
           if (polyline.Count < 2)
           {
               return null;
           }
           Vector2d tmp = polyline[polyline.Count - 1] - polyline[0];
           return tmp;
       }
   }
}
