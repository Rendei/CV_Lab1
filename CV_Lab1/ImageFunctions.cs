using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace CV_Lab1
{
    public class ImageFunctions
    {
        public enum ColorChannel
        {
            Red,
            Green,
            Blue
        }

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

        public static BitmapSource AdjustBrightness(BitmapSource source, ColorChannel channel, double brightnessValue)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                switch (channel)
                {
                    case ColorChannel.Red:
                        pixels[i + 2] = CheckByteRange(pixels[i + 2] + brightnessValue);
                        break;
                    case ColorChannel.Green:
                        pixels[i + 1] = CheckByteRange(pixels[i + 1] + brightnessValue);
                        break;
                    case ColorChannel.Blue:
                        pixels[i] = CheckByteRange(pixels[i] + brightnessValue);
                        break;
                }
            }

            BitmapSource adjustedImage = BitmapSource.Create(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null, pixels, stride);
            return adjustedImage;
        }

        public static BitmapSource AdjustContrast(BitmapSource source, double threshold)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);
            var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);

            for (int i = 0; i < pixels.Length; i += 4)
            {                
                double oldRed = pixels[i + 2];
                double oldGreen = pixels[i + 1];
                double oldBlue = pixels[i];

                double red = ((((oldRed / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                double green = ((((oldGreen / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                double blue = ((((oldBlue / 255.0) - 0.5) * contrast) + 0.5) * 255.0;

                pixels[i] = CheckByteRange(blue);
                pixels[i + 1] = CheckByteRange(green);
                pixels[i + 2] = CheckByteRange(red);               
            }

            BitmapSource adjustedImage = BitmapSource.Create(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null, pixels, stride);
            return adjustedImage;
        }

        public static BitmapSource GetNegativeImage(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            WriteableBitmap negativeImage = new WriteableBitmap(source);

            int width = negativeImage.PixelWidth;
            int height = negativeImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            negativeImage.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;
                    byte blue = (byte)(255 - pixels[index]);
                    byte green = (byte)(255 - pixels[index + 1]);
                    byte red = (byte)(255 - pixels[index + 2]);
                    pixels[index] = blue;
                    pixels[index + 1] = green;
                    pixels[index + 2] = red;
                }
            }

            negativeImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return negativeImage;
        }

        public static BitmapSource SwapColorChannels(BitmapSource source, int firstChannel, int secondChannel) //1 for green, 2 for red, 3 for blue         
        {
            WriteableBitmap swappedChannelsImage = new WriteableBitmap(source);

            int width = swappedChannelsImage.PixelWidth;
            int height = swappedChannelsImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            swappedChannelsImage.CopyPixels(pixels, stride, 0);
           
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;
                    byte firstValue = pixels[index + firstChannel];
                    byte secondValue = pixels[index + secondChannel];

                    pixels[index + firstChannel] = secondValue;
                    pixels[index + secondChannel] = firstValue;
                }
            }

            swappedChannelsImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return swappedChannelsImage;
        }

        public static BitmapSource FlipImageVertically(BitmapSource source)
        {
            WriteableBitmap rotatedImage = new WriteableBitmap(source);

            int width = rotatedImage.PixelWidth;
            int height = rotatedImage.PixelHeight;
            int stride = width * 4; 
            byte[] pixels = new byte[height * stride];
            rotatedImage.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int indexTop = (y * width + x) * 4;
                    int indexBottom = ((height - y - 1) * width + x) * 4;

                    byte blueTemp = pixels[indexTop];
                    byte greenTemp = pixels[indexTop + 1];
                    byte redTemp = pixels[indexTop + 2];

                    pixels[indexTop] = pixels[indexBottom];
                    pixels[indexTop + 1] = pixels[indexBottom + 1];
                    pixels[indexTop + 2] = pixels[indexBottom + 2];

                    pixels[indexBottom] = blueTemp;
                    pixels[indexBottom + 1] = greenTemp;
                    pixels[indexBottom + 2] = redTemp;
                }
            }

            rotatedImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return rotatedImage;
        }

        public static byte CheckByteRange(double value)
        {
            return (byte)Math.Min(Math.Max(value, 0), 255);
        }
    }
}
