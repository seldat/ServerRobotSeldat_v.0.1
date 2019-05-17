using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    public class SafeCircle
    {
        public EllipseGeometry ElG { get; set; }
        public Path path;
        public SafeCircle(Canvas canvas,Color color,int strokeThickness ) {
            ElG = new EllipseGeometry();
            path = new Path();
            path.Stroke = new SolidColorBrush(color);
            path.StrokeThickness = strokeThickness;
            path.Data = ElG;
            canvas.Children.Add(path);
        }
        public void Set(Point position,Point center,Point radius)
        {
            Canvas.SetLeft(path, position.X);
            Canvas.SetTop(path, position.Y);
            ElG.RadiusX = radius.X;
            ElG.RadiusY = radius.Y;
            ElG.Center = center;
        }
    }
}
