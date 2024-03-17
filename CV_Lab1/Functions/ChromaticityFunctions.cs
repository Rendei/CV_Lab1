using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CV_Lab1.Functions
{
    internal class ChromaticityFunctions
    {
        public static BitmapSource ApplyLogarithmicTransform(BitmapSource source)
        {
            WriteableBitmap logarithmicImage = new WriteableBitmap(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            double c = 255 / Math.Log(1 + pixels.Max());
            pixels = pixels.Select(pixel => (byte)(c * Math.Log(pixel))).ToArray();

            logarithmicImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return logarithmicImage;
        }

        public static BitmapSource ApplyExponentialTransform(BitmapSource source, double exponent)
        {
            WriteableBitmap exponentialImage = new WriteableBitmap(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            double maxValue = pixels.Max();
            double c = 255 / Math.Pow(maxValue, exponent);

            pixels = pixels.Select(pixel => ApplyExponential(pixel, exponent, c)).ToArray();

            exponentialImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return exponentialImage;
        }

        private static byte ApplyExponential(byte pixelValue, double exponent, double c)
        {
            double newValue = c * Math.Pow(pixelValue, exponent);
            return (byte)Math.Min(255, Math.Max(0, newValue));
        }

        public static BitmapSource ApplyBinaryTransform(BitmapSource source, byte threshold)
        {
            WriteableBitmap binaryImage = new WriteableBitmap(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte grayValue = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);

                byte binaryValue = (grayValue < threshold) ? (byte)0 : (byte)255;

                pixels[i] = binaryValue;     // Blue
                pixels[i + 1] = binaryValue; // Green
                pixels[i + 2] = binaryValue; // Red
            }

            binaryImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return binaryImage;
        }

        public static BitmapSource CutPixelRange(BitmapSource source, byte minThreshold, byte maxThreshold, byte constantValue = 0, bool keepOriginal = false)
        {
            WriteableBitmap cutImage = new WriteableBitmap(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte grayValue = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);

                if (grayValue < minThreshold || grayValue > maxThreshold)
                {
                    if (keepOriginal)
                    {
                        continue;
                    }
                    else
                    {
                        pixels[i] = constantValue;       // Blue
                        pixels[i + 1] = constantValue;   // Green
                        pixels[i + 2] = constantValue;   // Red
                    }
                }
                else
                {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                }
            }

            cutImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return cutImage;
        }
    }
}
