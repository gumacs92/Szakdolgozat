using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Szakdolgozat.Kinect.ToolBox
{
    public class SwipeGestureDetector : GestureDetector
    {
        public float SwipeMinimalLength { get; set; }
        public float SwipeMaximalDifference { get; set; }
        public int SwipeMininalDuration { get; set; }
        public int SwipeMaximalDuration { get; set; }
        
        public SwipeGestureDetector(Canvas gestureCanvas, int windowSize = 30)
        : base(gestureCanvas, windowSize)
        {
            SwipeMinimalLength = 0.45f;
            SwipeMaximalDifference = 0.2f;
            SwipeMininalDuration = 250;
            SwipeMaximalDuration = 650;
        }
        protected bool ScanPositions(
            Func<Vector, Vector, bool> differenceFunc,
            Func<Vector, Vector, bool> directionFunc,
            Func<Vector, Vector, bool> lengthFunc)
        {
            int start = 0;
            for (int index = 1; index < GestureEntities.Count - 1; index++)
            {
                if (!differenceFunc(GestureEntities[0].Position, GestureEntities[index].Position) ||
                !directionFunc(GestureEntities[index].Position, GestureEntities[index + 1].Position))
                {
                    start = index;
                }
                if (lengthFunc(GestureEntities[index].Position, GestureEntities[start].Position))
                {
                    double totalMilliseconds =
                    (GestureEntities[index].Time - GestureEntities[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= SwipeMininalDuration && totalMilliseconds <= SwipeMaximalDuration)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        protected override void LookForGesture()
        {
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalDifference,
            (p1, p2) => p2.X - p1.X > -0.01f,
            (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength))
            {
                RaiseGestureDetected("SwipeRight");
                return;
            }
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalDifference,
            (p1, p2) => p2.X - p1.X < 0.01f,
            (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength))
            {
                RaiseGestureDetected("SwipeLeft");
                return;
            }
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalDifference,
            (p1, p2) => p2.Y - p1.Y > -0.01f,
            (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength))
            {
                RaiseGestureDetected("SwipeUp");
                return;
            }

            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalDifference,
            (p1, p2) => p2.Y - p1.Y < 0.01f, 
            (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength))
            {
                RaiseGestureDetected("SwipeDown");
                return;
            }
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalDifference && Math.Abs(p2.Y - p1.Y) < SwipeMaximalDifference,
            (p1, p2) => p2.Z - p1.Z < 0.01f,
            (p1, p2) => Math.Abs(p2.Z - p1.Z) > SwipeMinimalLength))
            {
                RaiseGestureDetected("Push");
                return;
            }
        }
    }
}
