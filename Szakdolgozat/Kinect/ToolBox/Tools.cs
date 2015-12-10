using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Szakdolgozat.Kinect.ToolBox;
using System.Windows;

namespace Szakdolgozat.Kinect.ToolBox
{
    public static class Tools
    {
        public static Skeleton[] GetSkeletons(this SkeletonFrame frame)
        {
            if (frame == null)
                return null;
            var skeletons = new Skeleton[frame.SkeletonArrayLength];

            frame.CopySkeletonDataTo(skeletons);

            return skeletons;
        }

        public static Point Convert(this KinectSensor sensor, SkeletonPoint position)
        {
            float width = 0;
            float height = 0;
            float x = 0;
            float y = 0;
            if (sensor.ColorStream.IsEnabled)
            {

                ColorImagePoint colorPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(position, sensor.ColorStream.Format);
                x = colorPoint.X;
                y = colorPoint.Y;

                width = 640;
                height = 480;
            }
            else
            {
                width = 1;
                height = 1;
            }
            return new Point(x / width, y / height);
        }

        public static double Length(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
