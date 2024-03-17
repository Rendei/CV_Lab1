using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using static CV_Lab1.Functions.ImageFunctions;

namespace CV_Lab1.Functions
{
    class SmoothingFunctions
    {
        public static BitmapSource RectangularFilter(BitmapSource source)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);
            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index= (y * width + x) * 4;
                    byte neighborValue = GetRectangularNeighbors(source, 3, x, y);
                    pixels[index] = neighborValue;
                }
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return filteredImage;
        }

        private static byte GetRectangularNeighbors(BitmapSource source, int windowSize, int x, int y)
        {
            List<double> neighborValues = new List<double>();           
            int halfWindowSize = windowSize / 2;

            for (int i = -halfWindowSize; i <= halfWindowSize; i++)
            {
                for (int j = -halfWindowSize; j <= halfWindowSize; j++)
                {
                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX >= 0 && neighborX < source.PixelWidth && neighborY >= 0 && neighborY < source.PixelHeight)
                    {
                        neighborValues.Add(GetAverageIntensity(GetPixelColor(source, neighborX, neighborY)));
                    }
                }
            }

            return (byte)(neighborValues.Sum() / Math.Pow(windowSize, 2));
        }

        public static BitmapSource MedianFilter(BitmapSource source)
        {

        }
    }
}
