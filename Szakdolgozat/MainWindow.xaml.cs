using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Szakdolgozat.Kinect.ToolBox;
using Szakdolgozat.Model;

namespace Szakdolgozat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _kinectSensor;
        private KinectColorStreamManager _colorManager;
        private KinectSkeletonDisplayManager _skeletonManager;

        private SwipeGestureDetector _swipeGestureRecognizer;

        private readonly ContextTracker _contextTracker = new ContextTracker();

        private bool _seated;

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += GamePanel.UserControl_KeyDown;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            GameLogic.GetInstance().NewGame();
            GamePanel.NewGame();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        _kinectSensor = kinect;
                        break;
                    }
                }
                if (KinectSensor.KinectSensors.Count == 0)
                    MessageBox.Show("No Kinect found");
                else
                    Initialize(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (_kinectSensor == null)
                    {
                        _kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (_kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (_kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no longer powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Initialize()
        {
            if (_kinectSensor == null)
                return;

            _colorManager = new KinectColorStreamManager();
            _skeletonManager = new KinectSkeletonDisplayManager(_kinectSensor, SkeletonCanvas);

            _swipeGestureRecognizer = new SwipeGestureDetector(GesturesCanvas);
            _swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;

            _seated = true;

            KinectDisplay.DataContext = _colorManager;

            _kinectSensor.SkeletonStream.Enable();
            _kinectSensor.SkeletonStream.EnableTrackingInNearRange = _seated;
            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
              
            
            _kinectSensor.SkeletonFrameReady += KinectSensor_SkeletonFrameReady;
            _kinectSensor.ColorFrameReady += KinectSensor_ColorFrameReady;
            _kinectSensor.Start();

        }

        void OnGestureDetected(string gesture)
        {
            if (gesture.Equals("SwipeRight"))
            {
                GamePanel.SetMovingDirAndControl(Direction.RIGHT);
            }
            if (gesture.Equals("SwipeLeft"))
            {
                GamePanel.SetMovingDirAndControl(Direction.LEFT);
            }
            if (gesture.Equals("SwipeUp"))
            {
                GamePanel.SetMovingDirAndControl(Direction.UP);
            }
            if (gesture.Equals("SwipeDown"))
            {
                GamePanel.SetMovingDirAndControl(Direction.DOWN);
            }
            if (gesture.Equals("Push"))
            {
                GameLogic.GetInstance().NewGame();
                GamePanel.NewGame();
            }
        }

        private void Clean()
        {
            if (_skeletonManager != null)
            {
                _skeletonManager = null;
            }

            if (_colorManager != null)
            {
                _colorManager = null;
            }

            if (_swipeGestureRecognizer != null)
            {
                _swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;
                _swipeGestureRecognizer = null;
            }
            

            if (_kinectSensor != null)
            {
                _kinectSensor.SkeletonStream.Disable();
                _kinectSensor.ColorStream.Disable();
                _kinectSensor.SkeletonFrameReady -= KinectSensor_SkeletonFrameReady;
                _kinectSensor.ColorFrameReady -= KinectSensor_ColorFrameReady;
                _kinectSensor.Stop();
                _kinectSensor = null;
            }
        }
        

        private void KinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;
                _colorManager.Update(frame);
            }
        }

        private void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                ProcessFrame(frame);
            }
        }

        void ProcessFrame(SkeletonFrame frame)
        {
            Skeleton[] skeletons = frame.GetSkeletons();
            foreach (var skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                _contextTracker.Add(Kinect.ToolBox.Vector.ToVector(skeleton.Position), skeleton.TrackingId);
                if (!_contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                    continue;

                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;
                    if (joint.JointType == JointType.HandRight)
                    {
                        _swipeGestureRecognizer.Add(joint.Position, _kinectSensor);
                    }
                }
            }

            _skeletonManager.Draw(skeletons, _seated);
        }
        
    }
}
