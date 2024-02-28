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

        public static void ChangeColorChannelBrightness(BitmapSource source, int colorChannelOffset, int additionValue)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            byte[] pixels = new byte[width * height * 4];

            source.CopyPixels(pixels, width, 0);

            for (int i = 0; i < width; i+= colorChannelOffset)
            {
                for (int j = 0; j < height; j+= colorChannelOffset)
                {
                    int index = (j * width + i) * 4;

                    if (pixels[index] + additionValue > 255)
                        pixels[index] = 255;
                    else if (pixels[index] - additionValue < 0)
                        pixels[index] = 0;                 
                }
            }
        }
    }
}
