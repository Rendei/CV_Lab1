using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CV_Lab1
{
    public class ImageFunctions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public float Blue;
            public float Green;
            public float Red;
            public byte Alpha;

            public static PixelColor operator /(PixelColor c, byte divisor) => new PixelColor()
            {
                Blue = (float)(c.Blue / divisor),
                Green = (float)(c.Green / divisor),
                Red = (float)(c.Red / divisor),
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
    }
}
