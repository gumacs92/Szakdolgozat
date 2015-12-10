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
using Szakdolgozat.Model;

namespace Szakdolgozat.Kinect.ToolBox
{
    public class KinectSkeletonDisplayManager
    {
        readonly Canvas _rootCanvas;
        readonly KinectSensor _kinectSensor;
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredBoneBrush = Brushes.Gray;
        private readonly Brush trackedBoneBrush = Brushes.Green;

        public KinectSkeletonDisplayManager(KinectSensor kinectSensor, Canvas root)
        {
            _rootCanvas = root;
            _kinectSensor = kinectSensor;
    }
        public void Draw(Skeleton[] skeletons, bool seated)
        {
            _rootCanvas.Children.Clear();
            foreach (Skeleton skeleton in skeletons)
            {
                this.DrawBone(skeleton, JointType.Head, JointType.ShoulderCenter);
                this.DrawBone(skeleton, JointType.ShoulderCenter, JointType.ShoulderLeft);
                this.DrawBone(skeleton, JointType.ShoulderCenter, JointType.ShoulderRight);
                this.DrawBone(skeleton, JointType.ShoulderCenter, JointType.Spine);
                this.DrawBone(skeleton, JointType.Spine, JointType.HipCenter);

                
                this.DrawBone(skeleton, JointType.ShoulderLeft, JointType.ElbowLeft);
                this.DrawBone(skeleton, JointType.ElbowLeft, JointType.WristLeft);
                this.DrawBone(skeleton, JointType.WristLeft, JointType.HandLeft);
                
                this.DrawBone(skeleton, JointType.ShoulderRight, JointType.ElbowRight);
                this.DrawBone(skeleton, JointType.ElbowRight, JointType.WristRight);
                this.DrawBone(skeleton, JointType.WristRight, JointType.HandRight);

                if (!seated)
                {
                    this.DrawBone(skeleton, JointType.HipCenter, JointType.HipLeft);
                    this.DrawBone(skeleton, JointType.HipCenter, JointType.HipRight);
                    
                    this.DrawBone(skeleton, JointType.HipLeft, JointType.KneeLeft);
                    this.DrawBone(skeleton, JointType.KneeLeft, JointType.AnkleLeft);
                    this.DrawBone(skeleton, JointType.AnkleLeft, JointType.FootLeft);
                   
                    this.DrawBone(skeleton, JointType.HipRight, JointType.KneeRight);
                    this.DrawBone(skeleton, JointType.KneeRight, JointType.AnkleRight);
                    this.DrawBone(skeleton, JointType.AnkleRight, JointType.FootRight);
                }
                
                
                foreach (Joint joint in skeleton.Joints)
                {
                    Brush drawBrush = null;

                    if (joint.TrackingState == JointTrackingState.Tracked)
                    {
                        drawBrush = this.trackedJointBrush;
                    }
                    else if (joint.TrackingState == JointTrackingState.Inferred)
                    {
                        drawBrush = this.inferredJointBrush;
                    }

                    if (drawBrush != null)
                    {
                        if(joint.JointType == JointType.Head)
                        {
                            DrawHead(joint, skeleton.Joints.First(p => p.JointType == JointType.ShoulderCenter), drawBrush);
                        }
                        else
                        {
                            if(!seated || joint.JointType == JointType.ShoulderCenter
                                || joint.JointType == JointType.ShoulderLeft || joint.JointType == JointType.ShoulderRight
                                || joint.JointType == JointType.ElbowLeft || joint.JointType == JointType.ElbowRight
                                || joint.JointType == JointType.WristLeft || joint.JointType == JointType.WristRight
                                || joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight
                                || joint.JointType == JointType.Spine || joint.JointType == JointType.HipCenter)
                            DrawJoint(joint, drawBrush);
                        }
                    }
                }
            }
        }

        private void DrawBone(Skeleton skeleton, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];
            
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }
            
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }
            
            Brush drawBrush = this.inferredBoneBrush;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawBrush = this.trackedBoneBrush;
            }
            Point point0 = GetCoordinates(joint0);
            Point point1 = GetCoordinates(joint1);
            Line line = new Line
            {
                X1 = point0.X,
                Y1 = point0.Y,
                X2 = point1.X,
                Y2 = point1.Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = drawBrush,
                StrokeLineJoin = PenLineJoin.Round
            };
            _rootCanvas.Children.Add(line);
        }

        Point GetCoordinates(Joint joint)
        {
            Point p = _kinectSensor.Convert(joint.Position);
            var x = (float)(p.X * _rootCanvas.ActualWidth);
            var y = (float)(p.Y * _rootCanvas.ActualHeight);
            return new Point(x, y);
        }

        void DrawHead(Joint head, Joint shouldercenter, Brush brushType)
        {
            Point phead = GetCoordinates(head);
            Point pshouldercenter = GetCoordinates(shouldercenter);
            double diameter = Math.Abs(Tools.Length(phead, pshouldercenter));

            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = brushType,
                StrokeLineJoin = PenLineJoin.Round
            };
            Canvas.SetLeft(ellipse, phead.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, phead.Y - ellipse.Height / 2);
            _rootCanvas.Children.Add(ellipse);
        }

        void DrawJoint(Joint joint, Brush brushType)
        {
            Point pjoint = GetCoordinates(joint);
            const double diameter = 8;
            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = brushType,
                StrokeLineJoin = PenLineJoin.Round
            };
            Canvas.SetLeft(ellipse, pjoint.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, pjoint.Y - ellipse.Height / 2);
            _rootCanvas.Children.Add(ellipse);
        }
    }
}
