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
        public MainWindow()
        {
            InitializeComponent();
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

        private void ConvertImageToBW(object sender, RoutedEventArgs e)
        {
            var imageSource = userImg.Source;
            
            var pixels = ImageFunctions.GetPixels((BitmapSource)imageSource);
            byte[] bytes = new byte[pixels.GetLength(0) * pixels.GetLength(1) * 4];
            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    pixels[i, j] /= 255;
                    bytes[j + i * j] = pixels[i, j].Red;
                    bytes[j + i * j + 1] = pixels[i, j].Green;
                    bytes[j + i * j + 2] = pixels[i, j].Blue;
                    bytes[j + i * j + 3] = pixels[i, j].Alpha;
                }
            }

            //var bitmap = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new System.IO.MemoryStream(bytes);
            image.EndInit();
            userImgBW.Source = image;

        }
    }
}