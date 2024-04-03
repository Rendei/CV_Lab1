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

namespace CV_Lab1.Functions
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
                Alpha = c.Alpha,
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
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x >= bitmap.PixelWidth)
                x = bitmap.PixelWidth - 1;
            if (y >= bitmap.PixelHeight)
                y = bitmap.PixelHeight - 1;

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

                double red = ((oldRed / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                double green = ((oldGreen / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                double blue = ((oldBlue / 255.0 - 0.5) * contrast + 0.5) * 255.0;

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

        public static BitmapSource RemoveNoise(BitmapSource image, int neighborCount)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;

            WriteableBitmap resultBitmap = new WriteableBitmap(image);

            byte[] newPixels = new byte[width * height * 4];

            image.CopyPixels(newPixels, width * 4, 0);

            int[] xOffset = new int[] { };
            int[] yOffset = new int[] { };

            if (neighborCount == 4)
            {
                // 4 neighbors: top, bottom, left, right
                xOffset = new int[] { 0, 0, -1, 1 };
                yOffset = new int[] { -1, 1, 0, 0 };
            }
            else if (neighborCount == 8)
            {
                // 8 neighbors: top, bottom, left, right, top-left, top-right, bottom-left, bottom-right
                xOffset = new int[] { 0, 0, -1, 1, -1, 1, -1, 1 };
                yOffset = new int[] { -1, 1, 0, 0, -1, -1, 1, 1 };
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;

                    int sumR = 0;
                    int sumG = 0;
                    int sumB = 0;
                    int count = 0;

                    for (int i = 0; i < xOffset.Length; i++)
                    {
                        int neighborX = x + xOffset[i];
                        int neighborY = y + yOffset[i];

                        if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                        {
                            int neighborIndex = (neighborY * width + neighborX) * 4;

                            sumR += newPixels[neighborIndex + 2]; // Red channel
                            sumG += newPixels[neighborIndex + 1]; // Green channel
                            sumB += newPixels[neighborIndex]; // Blue channel
                            count++;
                        }
                    }

                    byte avgR = (byte)(sumR / count);
                    byte avgG = (byte)(sumG / count);
                    byte avgB = (byte)(sumB / count);

                    newPixels[index] = avgB;
                    newPixels[index + 1] = avgG;
                    newPixels[index + 2] = avgR;
                    newPixels[index + 3] = 255;
                }
            }

            resultBitmap.WritePixels(new Int32Rect(0, 0, width, height), newPixels, width * 4, 0);

            return resultBitmap;
        }

        public static BitmapSource NormalizePixelValues(BitmapSource source)
        {
            WriteableBitmap normalizedImage = new WriteableBitmap(source);
            int width = normalizedImage.PixelWidth;
            int height = normalizedImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            normalizedImage.CopyPixels(pixels, stride, 0);

            double scale = 100 / 255.0;

            for (int y = 0; y < source.PixelHeight; y++)
            {
                for (int x = 0; x < source.PixelWidth; x++)
                {
                    int index = (y * source.PixelWidth + x) * 4;
                    Color originalColor = GetPixelColor(source, x, y);

                    // Normalize each channel value to the range [0, 100]
                    byte normalizedR = (byte)(originalColor.R * scale);
                    byte normalizedG = (byte)(originalColor.G * scale);
                    byte normalizedB = (byte)(originalColor.B * scale);                   

                    pixels[index] = normalizedB;
                    pixels[index + 1] = normalizedG;
                    pixels[index + 2] = normalizedR;
                }
            }

            normalizedImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return normalizedImage;
        }

        public static BitmapSource CalculateDifferenceMap(BitmapSource firstImage, BitmapSource secondImage)
        {
            WriteableBitmap differenceMap = new WriteableBitmap(firstImage);
            int width = firstImage.PixelWidth;
            int height = firstImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;
                    Color firstColor = GetPixelColor(firstImage, x, y);
                    Color secondColor = GetPixelColor(secondImage, x, y);
                   
                    byte differenceR = (byte)Math.Abs(firstColor.R - secondColor.R);
                    byte differenceG = (byte)Math.Abs(firstColor.G - secondColor.G);
                    byte differenceB = (byte)Math.Abs(firstColor.B - secondColor.B);                    

                    pixels[index] = differenceB;
                    pixels[index + 1] = differenceG;
                    pixels[index + 2] = differenceR;
                }
            }

            differenceMap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return differenceMap;
        }

        public static byte[] Convolve(byte[] pixels, int width, int height, double[,] kernel, int kernelSize)
        {
            byte[] result = new byte[pixels.Length];

            int kernelRadius = kernelSize / 2;

            for (int y = kernelRadius; y < height - kernelRadius; y++)
            {
                for (int x = kernelRadius; x < width - kernelRadius; x++)
                {
                    double blue = 0.0, green = 0.0, red = 0.0;

                    for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
                    {
                        for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                        {
                            int imageX = (x - kernelRadius + filterX + width) % width;
                            int imageY = (y - kernelRadius + filterY + height) % height;

                            byte b = pixels[(imageY * width + imageX) * 4];
                            byte g = pixels[(imageY * width + imageX) * 4 + 1];
                            byte r = pixels[(imageY * width + imageX) * 4 + 2];

                            blue += b * kernel[filterY + kernelRadius, filterX + kernelRadius];
                            green += g * kernel[filterY + kernelRadius, filterX + kernelRadius];
                            red += r * kernel[filterY + kernelRadius, filterX + kernelRadius];
                        }
                    }

                    int index = (y * width + x) * 4;
                    result[index] = CheckByteRange(blue);
                    result[index + 1] = CheckByteRange(green);
                    result[index + 2] = CheckByteRange(red);
                    result[index + 3] = pixels[index + 3];
                }
            }

            return result;
        }

        public static byte CheckByteRange(double value)
        {
            return (byte)Math.Min(Math.Max(value, 0), 255);
        }

        public static byte GetAverageIntensity(Color color)
        {
            return (byte)((color.R + color.G + color.B) / 3);
        }
    }
}
