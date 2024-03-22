using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static CV_Lab1.Functions.ImageFunctions;
    
namespace CV_Lab1.Functions
{
    internal class AntialiasingFunction
    {
        public static BitmapSource ApplyUnsharpMasking(BitmapSource source, BitmapSource changedSource, double lambda)
        {
            WriteableBitmap maskedImage = new WriteableBitmap(source);

            int width = maskedImage.PixelWidth;
            int height = maskedImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            maskedImage.CopyPixels(pixels, stride, 0);
            BitmapSource differenceMap = CalculateDifferenceMap(source, changedSource);


            for (int y = 0; y < source.PixelHeight; y++)
            {
                for (int x = 0; x < source.PixelWidth; x++)
                {
                    int index = (y * source.PixelWidth + x) * 4;                    
                    Color differencePixel = GetPixelColor(differenceMap, x, y);

                    pixels[index] += (byte)(differencePixel.B * lambda);
                    pixels[index + 1] += (byte)(differencePixel.G * lambda);
                    pixels[index + 2] += (byte)(differencePixel.R * lambda);                 
                }
            }

            maskedImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return maskedImage;
        }
    }
}
