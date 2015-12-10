using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Szakdolgozat.Kinect.ToolBox
{
    public class GesturePoint
    {
        public DateTime Time { get; set; }
        public Vector Position { get; set; }
        public Ellipse DisplayEllipse { get; set; }
    }
}
