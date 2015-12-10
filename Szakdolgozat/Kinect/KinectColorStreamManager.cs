using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Szakdolgozat.Model
{
    public class KinectColorStreamManager : KinectNotifier
    {
        public WriteableBitmap Bitmap { get; private set; }

        public void Update(ColorImageFrame frame)
        {
            var pixelData = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixelData);
            if (Bitmap == null)
            {
                Bitmap = new WriteableBitmap(frame.Width, frame.Height,
                96, 96, PixelFormats.Bgr32, null);
            }
            int stride = Bitmap.PixelWidth * Bitmap.Format.BitsPerPixel / 8;
            Int32Rect dirtyRect = new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight);
            Bitmap.WritePixels(dirtyRect, pixelData, stride, 0);
            RaisePropertyChanged(() => Bitmap);
        }
    }
}
