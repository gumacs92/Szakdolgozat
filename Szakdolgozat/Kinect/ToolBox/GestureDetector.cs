using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Szakdolgozat.Kinect.ToolBox
{
    public abstract class GestureDetector
    {
        public Canvas GestureCanvas { get; set; }
        public Color DisplayColor { get; set; }
        public int MinimalPeriodBetweenGestures { get; set; }

        readonly List<GesturePoint> gestureEntities = new List<GesturePoint>();

        public event Action<string> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize;
        protected List<GesturePoint> GestureEntities
        {
            get { return gestureEntities; }
        }
        public int WindowSize
        {
            get { return windowSize; }
        }

        protected GestureDetector(Canvas gestureCanvas, int windowSize = 20)
        {
            this.GestureCanvas = gestureCanvas;
            this.windowSize = windowSize;
            MinimalPeriodBetweenGestures = 1250;
            DisplayColor = Colors.Red;
        }
        
        public virtual void Add(SkeletonPoint position, KinectSensor sensor)
        {
            GesturePoint entity = new GesturePoint { Position = Vector.ToVector(position), Time = DateTime.Now };
            GestureEntities.Add(entity);
            if (GestureCanvas != null)
            {
                entity.DisplayEllipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(DisplayColor),
                    StrokeLineJoin = PenLineJoin.Round
                };

                Point point = Tools.Convert(sensor, position);
                float x = (float)(point.X * GestureCanvas.ActualWidth);
                float y = (float)(point.Y * GestureCanvas.ActualHeight);
                Canvas.SetLeft(entity.DisplayEllipse, x - entity.DisplayEllipse.Width / 2);
                Canvas.SetTop(entity.DisplayEllipse, y - entity.DisplayEllipse.Height / 2);
                GestureCanvas.Children.Add(entity.DisplayEllipse);
            }
            if (GestureEntities.Count > WindowSize)
            {
                GesturePoint entryToRemove = GestureEntities[0];
                if (GestureCanvas != null)
                {
                    GestureCanvas.Children.Remove(entryToRemove.DisplayEllipse);
                }
                GestureEntities.Remove(entryToRemove);
            }
            LookForGesture();
        }

        protected abstract void LookForGesture();

        protected void RaiseGestureDetected(string gesture)
        {
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                    OnGestureDetected(gesture);
                lastGestureDate = DateTime.Now;
            }
            GestureEntities.ForEach(e =>
            {
                if (GestureCanvas != null)
                    GestureCanvas.Children.Remove(e.DisplayEllipse);
            });
            GestureEntities.Clear();
        }
    }
}
