using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using static CV_Lab1.Functions.ImageFunctions;

namespace CV_Lab1.Functions
{
    class SmoothingFunctions
    {
        public static BitmapSource ApplyRectangularFilter(BitmapSource source)
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
                    int index = (y * width + x) * 4;
                    byte neighborValue = GetRectangularNeighbors(source, 3, x, y);
                    pixels[index] = neighborValue;
                    pixels[index + 1] = neighborValue;
                    pixels[index + 2] = neighborValue;
                    pixels[index + 3] = 255;
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

        public static BitmapSource ApplyMedianFilter(BitmapSource source)
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
                    int index = (y * width + x) * 4;
                    Color neighborValue = GetMedianNeighbors(source, 3, x, y);
                    pixels[index] = neighborValue.B;
                    pixels[index + 1] = neighborValue.G;
                    pixels[index + 2] = neighborValue.R;
                    pixels[index + 3] = 255;
                }
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return filteredImage;
        }

        private static Color GetMedianNeighbors(BitmapSource source, int windowSize, int x, int y)
        {
            List<Color> neighborValues = new List<Color>();
            int halfWindowSize = windowSize / 2;

            for (int i = -halfWindowSize; i <= halfWindowSize; i++)
            {
                for (int j = -halfWindowSize; j <= halfWindowSize; j++)
                {
                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX >= 0 && neighborX < source.PixelWidth && neighborY >= 0 && neighborY < source.PixelHeight)
                    {
                        neighborValues.Add(GetPixelColor(source, neighborX, neighborY));
                    }
                }
            }
            neighborValues.Sort((pixel1, pixel2) => GetAverageIntensity(pixel1) > GetAverageIntensity(pixel2) ? 1 : -1);

            return neighborValues[neighborValues.Count / 2 + 1];
        }

        public static BitmapSource ApplyGaussianFilter(BitmapSource source, double sigma)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];

            source.CopyPixels(pixels, stride, 0);
            int kernelSize = (int)(6 * sigma - 1);
            kernelSize = kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize;
            double[,] kernel = CreateGaussianKernel(kernelSize, sigma);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double[] sum = { 0, 0, 0 };
                    double weightSum = 0;

                    for (int i = 0; i < kernelSize; i++)
                    {
                        for (int j = 0; j < kernelSize; j++)
                        {
                            int offsetX = x - kernelSize / 2 + i;
                            int offsetY = y - kernelSize / 2 + j;

                            if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                            {
                                int kernelPixelIndex = (offsetY * width + offsetX) * 4;

                                for (int c = 0; c < 3; c++)
                                {
                                    double kernelValue = kernel[i, j];
                                    byte pixelValue = pixels[kernelPixelIndex + c];
                                    sum[c] += pixelValue * kernelValue;
                                }

                                weightSum += kernel[i, j];
                            }
                        }
                    }

                    int index = (y * width + x) * 4; 
                    //normalize pixel values
                    
                    byte newValueB = CheckByteRange(sum[0] / weightSum);
                    byte newValueG = CheckByteRange(sum[1] / weightSum);
                    byte newValueR = CheckByteRange(sum[2] / weightSum);
                    pixels[index] = newValueB;                    
                    pixels[index + 1] = newValueG;                    
                    pixels[index + 2] = newValueR;                    
                }
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return filteredImage;
        }

        private static double[,] CreateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sumTotal = 0;

            int kernelRadius = size / 2;
            double distance;
            double coefficient = 1.0 / (2.0 * Math.PI * sigma * sigma);

            for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (sigma * sigma));
                    kernel[filterY + kernelRadius, filterX + kernelRadius] = coefficient * Math.Exp(-distance);
                    sumTotal += kernel[filterY + kernelRadius, filterX + kernelRadius];
                }
            }

            //normalize kernel 
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] = kernel[y, x] / sumTotal;
                }
            }

            return kernel;
        }

        public static BitmapSource ApplySigmaFilter(BitmapSource source, double sigma)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            int radius = (int)Math.Ceiling(3 * sigma);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int minX = Math.Max(0, x - radius);
                    int minY = Math.Max(0, y - radius);
                    int maxX = Math.Min(width - 1, x + radius);
                    int maxY = Math.Min(height - 1, y + radius);

                    int[] neighborhoodValues = new int[(maxX - minX + 1) * (maxY - minY + 1)];
                    int index = 0;
                    for (int j = minY; j <= maxY; j++)
                    {
                        for (int i = minX; i <= maxX; i++)
                        {
                            int pixelIndex = (j * width + i) * 4;
                            byte pixelValue = pixels[pixelIndex];
                            neighborhoodValues[index++] = pixelValue;
                        }
                    }

                    //Array.Sort(neighborhoodValues);                    
                    //byte medianValue = (byte)neighborhoodValues[neighborhoodValues.Length / 2 + 1];

                    byte meanValue = (byte)neighborhoodValues.Average();

                    int pixelIndexToUpdate = (y * width + x) * 4;
                    pixels[pixelIndexToUpdate] = meanValue;
                    pixels[pixelIndexToUpdate + 1] = meanValue;
                    pixels[pixelIndexToUpdate + 2] = meanValue;
                }
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return filteredImage;
        }
    }
}
