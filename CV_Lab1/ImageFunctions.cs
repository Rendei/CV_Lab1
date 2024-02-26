using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace CV_Lab1
{
    public class ImageFunctions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;

            public static PixelColor operator /(PixelColor c, byte divisor) => new PixelColor()
            {
                Blue = (byte)(c.Blue / divisor),
                Green = (byte)(c.Green / divisor),
                Red = (byte)(c.Red / divisor),
                Alpha = (byte)(c.Alpha),
            };
        }

        public static PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];
           
            source.CopyPixels1(result, width * 4, 0);
            return result;        
        }

        public static Color GetPixelColor(BitmapSource bitmap, int x, int y)
        {
            if (x == bitmap.PixelWidth)
                x -= 1;
            if (y == bitmap.PixelHeight)
                y -= 1;
            byte[] pixel = new byte[4];            
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
            return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
        }
    }
}
