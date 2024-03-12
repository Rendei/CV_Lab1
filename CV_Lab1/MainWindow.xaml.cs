using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CV_Lab1.ImageFunctions;

namespace CV_Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image changedImg;
        private double averageWindowPixelValue;
        private double stdWindowPixelValue;

        public MainWindow()
        {
            InitializeComponent();
            changedImg = new Image();
            DataContext = this;
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "Image Files (*.BMP;*.PNG)|*.BMP;*.PNG"
            };
            fileDialog.DefaultExt = ".png";
            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                SetImage(fileDialog.FileName);
                var imageSource = (BitmapSource)userImg.Source;
                ConvertImageToBW(imageSource);
                imgBrightnessProfileBtn.IsEnabled = true;
                imgChangeBtn.IsEnabled = true;
                imgContrastMapBtn.IsEnabled = true;
                imgChannelsBtn.IsEnabled = true;
                //imgWindowPreviewBtn.IsEnabled = true;
                //new HistogramsWindow(redBitmap, greenBitmap, blueBitmap).Show();
            }
        }

        private void SetImage(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath);
            bitmapImage.DecodePixelWidth = 500;
            bitmapImage.DecodePixelHeight = 375;
            bitmapImage.EndInit();

            userImg.Source = bitmapImage;
            changedImg.Source = bitmapImage;
        }

        private void ConvertImageToBW(BitmapSource image)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            byte[] pixels = new byte[width * height * 4];

            image.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = (j * width + i) * 4;

                    byte luminance = (byte)(0.2126 * pixels[index + 2] + 0.7152 * pixels[index + 1] + 0.0722 * pixels[index]); // https://en.wikipedia.org/wiki/Grayscale

                    pixels[index] = luminance; // Blue
                    pixels[index + 1] = luminance; // Green
                    pixels[index + 2] = luminance; // Red
                }
            }

            BitmapSource grayscaleBitmap = BitmapSource.Create(width, height, 1, 1, PixelFormats.Bgra32, null, pixels, width * 4);
            userImgBW.Source = grayscaleBitmap;

        }


        private void userImg_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(userImg);

            Point frameTopLeft = MoveFrame(mousePos);
            Color centerPixel = MoveZoomedImage(mousePos,frameTopLeft, (int)frame.Width);
            double[] intensities = CalculatePixelIntensities((BitmapSource)userImg.Source, frameTopLeft, (int)frame.Width);
            (double average, double std) = CalculateAverageAndStd(intensities);
            averageWindowPixelValue = average;
            stdWindowPixelValue = std;
            MoveZoomedImageLabels(mousePos, frameTopLeft, centerPixel);           
        }

        private Point MoveFrame(Point point)
        {
            double frameLeft = point.X - frame.Width / 2; // center
            double frameTop = point.Y - frame.Height / 2; 
            int frameSize = (int)zoomedImageImg.Height;

            // Ensure the frame stays within the bounds of the image
            if (frameLeft < 0)
                frameLeft = 0;
            else if (frameLeft > userImg.ActualWidth - frameSize)
                frameLeft = userImg.ActualWidth - frameSize;

            if (frameTop < 0)
                frameTop = 0;
            else if (frameTop > userImg.ActualHeight - frameSize)
                frameTop = userImg.ActualHeight - frameSize;

            frame.Margin = new Thickness(frameLeft + userImg.Margin.Left, frameTop + userImg.Margin.Top, 0, 0);
            frame.Visibility = Visibility.Visible;

            return new Point(frameLeft, frameTop);
        }

        private Color MoveZoomedImage(Point mousePos, Point topLeft, int frameSize)
        {            
            BitmapSource originalSource = (BitmapSource)userImg.Source;            
            originalSource = new FormatConvertedBitmap(originalSource, PixelFormats.Bgra32, null, 0);

            int frameStartX = (int)Math.Round(topLeft.X);
            int frameStartY = (int)Math.Round(topLeft.Y);
            
            byte[] pixels = new byte[frameSize * frameSize * 4];

            for (int y = 0; y < frameSize; y++)
            {
                for (int x = 0; x < frameSize; x++)
                {
                    int pixelX = frameStartX + x;
                    int pixelY = frameStartY + y;

                    Color color = ImageFunctions.GetPixelColor(originalSource, pixelX, pixelY);

                    int destIndex = (y * frameSize + x) * 4;

                    pixels[destIndex] = color.B;
                    pixels[destIndex + 1] = color.G;
                    pixels[destIndex + 2] = color.R;
                    pixels[destIndex + 3] = color.A;

                }
            }
            
            WriteableBitmap zoomedImage = new WriteableBitmap(frameSize, frameSize, originalSource.DpiX, originalSource.DpiY, PixelFormats.Bgra32, null);
            zoomedImage.WritePixels(new Int32Rect(0, 0, frameSize, frameSize), pixels, frameSize * 4, 0);

            zoomedImageImg.Source = zoomedImage;
            zoomedImageImg.Margin = new Thickness(topLeft.X + userImg.Margin.Left, topLeft.Y + userImg.Margin.Top / 2.5 + zoomedImageImg.Height, 0, 0);
            Color centerPixelColor = ImageFunctions.GetPixelColor(originalSource, (int)Math.Floor(mousePos.X), (int)Math.Floor(mousePos.Y));

            return centerPixelColor;
        }

        private void MoveZoomedImageLabels(Point mousePos, Point topLeft, Color centerPixelColor)
        {
            centerPixelLabelTop.Visibility = Visibility.Visible;
            centerPixelLabelBottom.Visibility = Visibility.Visible;
            
            centerPixelLabelTop.Content = string.Format($"Координаты пикселя ({(int)mousePos.X}, {(int)mousePos.Y})\n" +
                $"Цвета каналов в данном пикселе Зелёный: {centerPixelColor.G}, Синий: {centerPixelColor.B}, Красный: {centerPixelColor.R}");
            //centerPixelLabelTop.Margin = new Thickness(topLeft.X, topLeft.Y - frame.Height / 2, 0, 0);
            centerPixelLabelTop.Margin = new Thickness(topLeft.X, topLeft.Y + frame.Height, 0, 0);

            centerPixelLabelBottom.Content = string.Format($"Интенсивность центрального пикселя равна: {(centerPixelColor.G + centerPixelColor.R + centerPixelColor.B) / 3}\n" +
                $"Среднее по окну: {this.averageWindowPixelValue} СКО: {this.stdWindowPixelValue}");
            centerPixelLabelBottom.Margin = new Thickness(topLeft.X, topLeft.Y + userImg.Margin.Top + frame.Height, 0, 0);
        }

        private double[] CalculatePixelIntensities(BitmapSource originalSource, Point topLeft, int frameSize)
        {
            int frameStartX = (int)Math.Round(topLeft.X);
            int frameStartY = (int)Math.Round(topLeft.Y);

            double[] intensities = new double[frameSize * frameSize];

            for (int y = 0; y < frameSize; y++)
            {
                for (int x = 0; x < frameSize; x++)
                {
                    int pixelX = frameStartX + x;
                    int pixelY = frameStartY + y;

                    Color color = ImageFunctions.GetPixelColor(originalSource, pixelX, pixelY);

                    double intensity = (color.R + color.G + color.B) / 3.0;
                    intensities[y * frameSize + x] = intensity;
                }
            }

            return intensities;
        }

        private (double, double) CalculateAverageAndStd(double[] intensities)
        {
            double averageIntensity = intensities.Average();

            double sumOfSquares = intensities.Select(val => (val - averageIntensity) * (val - averageIntensity)).Sum();
            double stdDev = Math.Sqrt(sumOfSquares / intensities.Length);

            return (averageIntensity, stdDev);
        }

        private void userImg_MouseLeave(object sender, MouseEventArgs e)
        {
            frame.Visibility = Visibility.Collapsed;
            centerPixelLabelTop.Visibility = Visibility.Collapsed;
            centerPixelLabelBottom.Visibility = Visibility.Collapsed;
            //zoomedImageImg.Source = null;
        }     

        private void imgChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            ImageChangerWindow imageChangerWindow = new ImageChangerWindow(userImg, changedImg);
            ChangedImageWindow changedImageWindow = new ChangedImageWindow(changedImg);
            changedImageWindow.Show();
            imageChangerWindow.ShowDialog();
            
        }

        private void imgBrightnessProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            BrightnessProfileWindow brightnessWindow = new BrightnessProfileWindow(userImg);
            brightnessWindow.ShowDialog();
        }

        

        private void imgContrastMapBtn_Click(object sender, RoutedEventArgs e)
        {
            ContrastMapWindow contrastMapWindow = new ContrastMapWindow(userImg);
            contrastMapWindow.ShowDialog();
        }

        private void imgChannelsBtn_Click(object sender, RoutedEventArgs e)
        {
            HistogramsWindow colorChannelsWindow = new HistogramsWindow(userImg);
            colorChannelsWindow.ShowDialog();
        }

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Space)
        //    {
        //        SaveFileDialog saveFileDialog = new SaveFileDialog()
        //        {
        //            FileName = $"C:\\Users\\фвьшт\\Downloads\\{DateTime.UtcNow.Ticks}.png",
        //            DefaultExt = ".png",
        //        };

        //        using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
        //        {
        //            var encoder = new PngBitmapEncoder();
        //            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)zoomedImageImg.Source));
        //            encoder.Save(stream);
        //        }

        //        MessageBox.Show($"Файл успешно сохранён с названием {saveFileDialog.SafeFileName}");
                
        //    }
        //}

        private void imgWindowPreviewBtn_Click(object sender, RoutedEventArgs e)
        {
            ImagePreviewWindow previewWindow = new ImagePreviewWindow();
            previewWindow.ShowDialog();
        }
    }

}
