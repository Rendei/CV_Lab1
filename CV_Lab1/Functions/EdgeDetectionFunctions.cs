using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static CV_Lab1.Functions.SmoothingFunctions;
using static CV_Lab1.Functions.ImageFunctions;

namespace CV_Lab1.Functions
{
    public class EdgeDetectionFunctions
    {
        public static BitmapSource ApplyLoG(BitmapSource source, int kernelSize, double sigma)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);

            // Apply Gaussian filter
            double[,] gaussianKernel = CreateGaussianKernel(kernelSize, sigma);
            byte[] blurredPixels = Convolve(pixels, width, height, gaussianKernel, kernelSize);

            // Apply Laplacian filter
            double[,] laplacianKernel = {
                {0,  1, 0},
                {1, -4, 1},
                {0,  1, 0}
            };
            byte[] logPixels = Convolve(blurredPixels, width, height, laplacianKernel, 3);

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), logPixels, stride, 0);

            return filteredImage;
        }

        public static BitmapSource ApplyDoG(BitmapSource source, int kernelSize, double sigma)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);
            
            double[,] gaussianKernel1 = CreateGaussianKernel(kernelSize, sigma);
            byte[] blurredPixels1 = Convolve(pixels, width, height, gaussianKernel1, kernelSize);

            double sigmaScaled = sigma * Math.Pow(1.6, 5);
            double[,] gaussianKernel2 = CreateGaussianKernel(kernelSize, sigmaScaled);
            byte[] blurredPixels2 = Convolve(pixels, width, height, gaussianKernel2, kernelSize);

            // Calculate DoG by subtracting the second blurred image from the first one
            byte[] dogPixels = new byte[blurredPixels1.Length];
            for (int i = 0; i < dogPixels.Length; i++)
            {
                dogPixels[i] = (byte)Math.Abs(blurredPixels1[i] - blurredPixels2[i]);
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), dogPixels, stride, 0);

            return filteredImage;
        }

        public static double GetSobelOperatorValue(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];

            source.CopyPixels(pixels, stride, 0);

            int[,] sobelX = {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 } };

            int[,] sobelY = {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 } };

            List<double> gradientValues = new List<double>();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int gradientX = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int offsetX = x + j;
                            int offsetY = y + i;
                            int pixelIndex = (offsetY * width + offsetX) * 4;
                            gradientX += sobelX[i + 1, j + 1] * pixels[pixelIndex]; //indexes are 0, 1, 2, but i is -1, 0, 1
                        }
                    }

                    int gradientY = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int offsetX = x + j;
                            int offsetY = y + i;
                            int pixelIndex = (offsetY * width + offsetX) * 4;
                            gradientY += sobelY[i + 1, j + 1] * pixels[pixelIndex];
                        }
                    }

                    int gradientMagnitude = (int)Math.Sqrt(gradientX * gradientX + gradientY * gradientY);

                    gradientValues.Add(CheckByteRange(gradientMagnitude));
                }
            }


            return gradientValues.Average();
        }

        public static BitmapSource ApplySobelOperator(BitmapSource source, int filterSize)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);

            double[,] sobelX = {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 } };

            double[,] sobelY = {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 } };

            switch (filterSize)
            {
                case 5:
                {
                    sobelX = new double[,] {
                        { -0.25, -0.2, 0, 0.2, 0.25 },
                        { -0.4, -0.5, 0, 0.5, 0.4 },
                        { -0.5, -1.0, 0, 1.0, 0.5 },
                        { -0.4, -0.5, 0, 0.5, 0.4 },
                        { -0.25, -0.2, 0, 0.2, 0.25 }
                    };

                    sobelY = new double[,]
                    {
                        { -0.25, -0.4, -0.5, -0.4, -0.25 },
                        { -0.2, -0.5, -1.0, -0.5, -0.2 },
                        { 0, 0, 0, 0, 0 },
                        { 0.2, 0.5, 1.0, 0.5, 0.2 },
                        { 0.25, 0.4, 0.5, 0.4, 0.25 }
                    };
                } break;
                case 7:
                {
                    sobelX = new double[,]
                    {
                        { -3.0 / 18.0, -2.0 / 13.0, -1.0 / 10.0, 0.0 / 9.0, 1.0 / 10.0, 2.0 / 13.0, 3.0 / 18.0 },
                        { -3.0 / 13.0, -2.0 / 8.0, -1.0 / 5.0, 0.0 / 4.0, 1.0 / 5.0, 2.0 / 8.0, 3.0 / 13.0 },
                        { -3.0 / 10.0, -2.0 / 5.0, -1.0 / 2.0, 0.0 / 1.0, 1.0 / 2.0, 2.0 / 5.0, 3.0 / 10.0 },
                        { -3.0 / 9.0, -2.0 / 4.0, -1.0 / 1.0, 0.0, 1.0 / 1.0, 2.0 / 4.0, 3.0 / 9.0 },
                        { -3.0 / 10.0, -2.0 / 5.0, -1.0 / 2.0, 0.0 / 1.0, 1.0 / 2.0, 2.0 / 5.0, 3.0 / 10.0 },
                        { -3.0 / 13.0, -2.0 / 8.0, -1.0 / 5.0, 0.0 / 4.0, 1.0 / 5.0, 2.0 / 8.0, 3.0 / 13.0 },
                        { -3.0 / 18.0, -2.0 / 13.0, -1.0 / 10.0, 0.0 / 9.0, 1.0 / 10.0, 2.0 / 13.0, 3.0 / 18.0 }
                    };


                    sobelY = new double[,] {
                        { -3.0 / 18.0, -3.0 / 13.0, -3.0 / 10.0, -3.0 / 9.0, -3.0 / 10.0, -3.0 / 13.0, -3.0 / 18.0 },
                        { -2.0 / 13.0, -2.0 / 8.0, -2.0 / 5.0, -2.0 / 4.0, -2.0 / 5.0, -2.0 / 8.0, -2.0 / 13.0 },
                        { -1.0 / 10.0, -1.0 / 5.0, -1.0 / 2.0, -1.0 / 1.0, -1.0 / 2.0, -1.0 / 5.0, -1.0 / 10.0 },
                        { 0.0 / 9.0, 0.0 / 4.0, 0.0, 0.0, 0.0, 0.0 / 4.0, 0.0 / 9.0 },
                        { 1.0 / 10.0, 1.0 / 5.0, 1.0 / 2.0, 1.0 / 1.0, 1.0 / 2.0, 1.0 / 5.0, 1.0 / 10.0 },
                        { 2.0 / 13.0, 2.0 / 8.0, 2.0 / 5.0, 2.0 / 4.0, 2.0 / 5.0, 2.0 / 8.0, 2.0 / 13.0 },
                        { 3.0 / 18.0, 3.0 / 13.0, 3.0 / 10.0, 3.0 / 9.0, 3.0 / 10.0, 3.0 / 13.0, 3.0 / 18.0 }
                    };
                } break;
            }

            byte[] grayscalePixels = ConvertToGrayscale(pixels);

            byte[] sobelXPixels = Convolve(grayscalePixels, width, height, sobelX, filterSize);
            byte[] sobelYPixels = Convolve(grayscalePixels, width, height, sobelY, filterSize);

            byte[] sobelPixels = CombineSobelImages(sobelXPixels, sobelYPixels);

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), sobelPixels, stride, 0);

            return filteredImage;
        }

        private static byte[] ConvertToGrayscale(byte[] pixels)
        {
            byte[] grayscalePixels = new byte[pixels.Length];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte gray = (byte)(0.2126 * pixels[i] + 0.7152 * pixels[i + 1] + 0.0722 * pixels[i + 2]);
                grayscalePixels[i] = gray;
                grayscalePixels[i + 1] = gray;
                grayscalePixels[i + 2] = gray;
                grayscalePixels[i + 3] = 255;
            }

            return grayscalePixels;
        }
        private static byte[] CombineSobelImages(byte[] sobelX, byte[] sobelY)
        {
            byte[] result = new byte[sobelX.Length];

            for (int i = 0; i < sobelX.Length; i++)
            {
                double val = Math.Sqrt(sobelX[i] * sobelX[i] + sobelY[i] * sobelY[i]);
                result[i] = (byte)Math.Min(val, 255);
            }

            return result;
        }

        public static BitmapSource ApplyLoGVideo(BitmapSource source, int kernelSize, double sigma)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);

            // Apply Gaussian filter
            double[,] gaussianKernel = CreateGaussianKernel(kernelSize, sigma);
            byte[] blurredPixels = ConvolveParallel(pixels, width, height, gaussianKernel, kernelSize);

            // Apply Laplacian filter
            double[,] laplacianKernel = {
                {0,  1, 0},
                {1, -4, 1},
                {0,  1, 0}
            };
            byte[] logPixels = ConvolveParallel(blurredPixels, width, height, laplacianKernel, 3);

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), logPixels, stride, 0);

            return filteredImage;
        }

        public static BitmapSource ApplySobelOperatorVideo(BitmapSource source, int filterSize)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);

            double[,] sobelX = {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 } };

            double[,] sobelY = {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 } };

            switch (filterSize)
            {
                case 5:
                    {
                        sobelX = new double[,] {
                        { -0.25, -0.2, 0, 0.2, 0.25 },
                        { -0.4, -0.5, 0, 0.5, 0.4 },
                        { -0.5, -1.0, 0, 1.0, 0.5 },
                        { -0.4, -0.5, 0, 0.5, 0.4 },
                        { -0.25, -0.2, 0, 0.2, 0.25 }
                    };

                        sobelY = new double[,]
                        {
                        { -0.25, -0.4, -0.5, -0.4, -0.25 },
                        { -0.2, -0.5, -1.0, -0.5, -0.2 },
                        { 0, 0, 0, 0, 0 },
                        { 0.2, 0.5, 1.0, 0.5, 0.2 },
                        { 0.25, 0.4, 0.5, 0.4, 0.25 }
                        };
                    }
                    break;
                case 7:
                    {
                        sobelX = new double[,]
                        {
                        { -3.0 / 18.0, -2.0 / 13.0, -1.0 / 10.0, 0.0 / 9.0, 1.0 / 10.0, 2.0 / 13.0, 3.0 / 18.0 },
                        { -3.0 / 13.0, -2.0 / 8.0, -1.0 / 5.0, 0.0 / 4.0, 1.0 / 5.0, 2.0 / 8.0, 3.0 / 13.0 },
                        { -3.0 / 10.0, -2.0 / 5.0, -1.0 / 2.0, 0.0 / 1.0, 1.0 / 2.0, 2.0 / 5.0, 3.0 / 10.0 },
                        { -3.0 / 9.0, -2.0 / 4.0, -1.0 / 1.0, 0.0, 1.0 / 1.0, 2.0 / 4.0, 3.0 / 9.0 },
                        { -3.0 / 10.0, -2.0 / 5.0, -1.0 / 2.0, 0.0 / 1.0, 1.0 / 2.0, 2.0 / 5.0, 3.0 / 10.0 },
                        { -3.0 / 13.0, -2.0 / 8.0, -1.0 / 5.0, 0.0 / 4.0, 1.0 / 5.0, 2.0 / 8.0, 3.0 / 13.0 },
                        { -3.0 / 18.0, -2.0 / 13.0, -1.0 / 10.0, 0.0 / 9.0, 1.0 / 10.0, 2.0 / 13.0, 3.0 / 18.0 }
                        };


                        sobelY = new double[,] {
                        { -3.0 / 18.0, -3.0 / 13.0, -3.0 / 10.0, -3.0 / 9.0, -3.0 / 10.0, -3.0 / 13.0, -3.0 / 18.0 },
                        { -2.0 / 13.0, -2.0 / 8.0, -2.0 / 5.0, -2.0 / 4.0, -2.0 / 5.0, -2.0 / 8.0, -2.0 / 13.0 },
                        { -1.0 / 10.0, -1.0 / 5.0, -1.0 / 2.0, -1.0 / 1.0, -1.0 / 2.0, -1.0 / 5.0, -1.0 / 10.0 },
                        { 0.0 / 9.0, 0.0 / 4.0, 0.0, 0.0, 0.0, 0.0 / 4.0, 0.0 / 9.0 },
                        { 1.0 / 10.0, 1.0 / 5.0, 1.0 / 2.0, 1.0 / 1.0, 1.0 / 2.0, 1.0 / 5.0, 1.0 / 10.0 },
                        { 2.0 / 13.0, 2.0 / 8.0, 2.0 / 5.0, 2.0 / 4.0, 2.0 / 5.0, 2.0 / 8.0, 2.0 / 13.0 },
                        { 3.0 / 18.0, 3.0 / 13.0, 3.0 / 10.0, 3.0 / 9.0, 3.0 / 10.0, 3.0 / 13.0, 3.0 / 18.0 }
                    };
                    }
                    break;
            }

            byte[] grayscalePixels = ConvertToGrayscale(pixels);

            byte[] sobelXPixels = ConvolveParallel(grayscalePixels, width, height, sobelX, filterSize);
            byte[] sobelYPixels = ConvolveParallel(grayscalePixels, width, height, sobelY, filterSize);

            byte[] sobelPixels = CombineSobelImages(sobelXPixels, sobelYPixels);

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), sobelPixels, stride, 0);

            return filteredImage;
        }
    }
}
