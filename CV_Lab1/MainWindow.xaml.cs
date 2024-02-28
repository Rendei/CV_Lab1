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

namespace CV_Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource redBitmap;
        private BitmapSource greenBitmap;
        private BitmapSource blueBitmap;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Частота",
                    Values = new ChartValues<byte> {0 , 0 ,1 , 3, 4, 0, 5}
                }
            };

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };

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
                DisplayColorChannels(imageSource);
            }
        }

        private void SetImage(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath);
            bitmapImage.DecodePixelWidth = 400;
            bitmapImage.DecodePixelHeight = 300;
            bitmapImage.EndInit();

            userImg.Source = bitmapImage;
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

        private void DisplayColorChannels(BitmapSource image)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;

            byte[] redChannel = new byte[width * height];
            byte[] greenChannel = new byte[width * height];
            byte[] blueChannel = new byte[width * height];

            byte[] pixels = new byte[width * height * 4];
            image.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4) // i += 4 because of RGBA
            {
                blueChannel[i / 4] = pixels[i];
                greenChannel[i / 4] = pixels[i + 1];
                redChannel[i / 4] = pixels[i + 2];
            }

            BitmapSource redBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, redChannel, width);
            BitmapSource greenBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, greenChannel, width);
            BitmapSource blueBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, blueChannel, width);

            RedChannelImage.Source = redBitmap;
            GreenChannelImage.Source = greenBitmap;
            BlueChannelImage.Source = blueBitmap;
        }

        private void userImg_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the mouse position relative to the image
            Point mousePos = e.GetPosition(userImg);

            Point frameTopLeft = MoveFrame(mousePos);
            Color centerPixel = MoveZoomedImage(mousePos,frameTopLeft, (int)frame.Width);
            MoveZoomedImageLabels(mousePos, frameTopLeft, centerPixel);
        }

        private Point MoveFrame(Point point)
        {
            // Calculate the frame position relative to the cursor position
            double frameLeft = point.X - frame.Width / 2; // Half the frame width
            double frameTop = point.Y - frame.Height / 2; // Half the frame height
            int frameSize = (int)zoomedImageImg.Height;
            // Ensure the frame stays within the bounds of the image
            if (frameLeft < 0)
                frameLeft = 0;
            else if (frameLeft > userImg.ActualWidth - frameSize) // Ensure the frame width doesn't exceed 110 pixels
                frameLeft = userImg.ActualWidth - frameSize;

            if (frameTop < 0)
                frameTop = 0;
            else if (frameTop > userImg.ActualHeight - frameSize) // Ensure the frame height doesn't exceed 110 pixels
                frameTop = userImg.ActualHeight - frameSize;

            // Update the Margin property of the frame to position it over the userImg image
            frame.Margin = new Thickness(frameLeft + userImg.Margin.Left, frameTop + userImg.Margin.Top, 0, 0);
            frame.Visibility = Visibility.Visible;

            return new Point(frameLeft, frameTop);
        }

        private Color MoveZoomedImage(Point mousePos, Point topLeft, int frameSize)
        {            
            BitmapSource originalSource = (BitmapSource)userImg.Source;            
            originalSource = new FormatConvertedBitmap(originalSource, PixelFormats.Bgra32, null, 0);

            // Calculate the coordinates of the top-left corner of the frame
            int frameStartX = (int)Math.Round(topLeft.X);
            int frameStartY = (int)Math.Round(topLeft.Y);
            
            byte[] pixels = new byte[frameSize * frameSize * 4];

            for (int y = 0; y < frameSize; y++)
            {
                for (int x = 0; x < frameSize; x++)
                {
                    int pixelX = frameStartX + x;
                    int pixelY = frameStartY + y;

                    // Get the color of the pixel from the original image
                    Color color = ImageFunctions.GetPixelColor(originalSource, pixelX, pixelY);

                    // Calculate destination index
                    int destIndex = (y * frameSize + x) * 4;

                    // Copy pixel values to the buffer
                    pixels[destIndex] = color.B;
                    pixels[destIndex + 1] = color.G;
                    pixels[destIndex + 2] = color.R;
                    pixels[destIndex + 3] = color.A;
                }
            }
            
            WriteableBitmap zoomedImage = new WriteableBitmap(frameSize, frameSize, originalSource.DpiX, originalSource.DpiY, PixelFormats.Bgra32, null);
            zoomedImage.WritePixels(new Int32Rect(0, 0, frameSize, frameSize), pixels, frameSize * 4, 0);

            // Display the zoomed image in the zoomedImageImg control
            zoomedImageImg.Source = zoomedImage;
            zoomedImageImg.Margin = new Thickness(topLeft.X + userImg.Margin.Left, topLeft.Y, 0, 0);
            Color centerPixelColor = ImageFunctions.GetPixelColor(originalSource, (int)Math.Floor(mousePos.X), (int)Math.Floor(mousePos.Y));

            return centerPixelColor;
        }

        private void MoveZoomedImageLabels(Point mousePos, Point topLeft, Color centerPixelColor)
        {
            centerPixelLabelTop.Visibility = Visibility.Visible;
            centerPixelLabelBottom.Visibility = Visibility.Visible;
            
            centerPixelLabelTop.Content = string.Format($"Координаты пикселя ({mousePos.X}, {mousePos.Y})\nЦвета каналов в данном пикселе Зелёный: {centerPixelColor.G}, Синий: {centerPixelColor.B}, Красный: {centerPixelColor.R}");
            centerPixelLabelTop.Margin = new Thickness(topLeft.X, topLeft.Y - frame.Height, 0, 0);

            centerPixelLabelBottom.Content = string.Format($"Интенсивность окна равна: {(centerPixelColor.G + centerPixelColor.R + centerPixelColor.B) / 3}");
            centerPixelLabelBottom.Margin = new Thickness(topLeft.X, topLeft.Y + frame.Height * 2 + 10, 0, 0);
        }

        private void userImg_MouseLeave(object sender, MouseEventArgs e)
        {
            frame.Visibility = Visibility.Collapsed;
            centerPixelLabelTop.Visibility = Visibility.Collapsed;
            centerPixelLabelBottom.Visibility = Visibility.Collapsed;
            zoomedImageImg.Source = null;
        }

        private void channelBrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            userImg.Source.SetCurrentValue(ActualWidthProperty, channelBrightnessSlider.Value);
        }
    }
}
