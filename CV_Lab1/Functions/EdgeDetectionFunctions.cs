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

        public static BitmapSource ApplyDoG(BitmapSource source, int kernelSize1, double sigma1)
        {
            WriteableBitmap filteredImage = new WriteableBitmap(source);

            int width = filteredImage.PixelWidth;
            int height = filteredImage.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            filteredImage.CopyPixels(pixels, stride, 0);

            // Apply first Gaussian filter
            double[,] gaussianKernel1 = CreateGaussianKernel(kernelSize1, sigma1);
            byte[] blurredPixels1 = Convolve(pixels, width, height, gaussianKernel1, kernelSize1);

            // Apply second Gaussian filter
            double[,] gaussianKernel2 = CreateGaussianKernel(kernelSize2, sigma2);
            byte[] blurredPixels2 = Convolve(pixels, width, height, gaussianKernel2, kernelSize2);

            // Calculate DoG by subtracting the second blurred image from the first one
            byte[] dogPixels = new byte[blurredPixels1.Length];
            for (int i = 0; i < dogPixels.Length; i++)
            {
                dogPixels[i] = (byte)Math.Abs(blurredPixels1[i] - blurredPixels2[i]);
            }

            filteredImage.WritePixels(new Int32Rect(0, 0, width, height), dogPixels, stride, 0);

            return filteredImage;
        }

        private static byte[] Convolve(byte[] pixels, int width, int height, double[,] kernel, int kernelSize)
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
    }
}
