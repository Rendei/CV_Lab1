using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CV_Lab1
{
    /// <summary>
    /// Логика взаимодействия для ImagePreviewWindow.xaml
    /// </summary>
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow()
        {
            InitializeComponent();
        }

        private void uniformHistogramButton_Click(object sender, RoutedEventArgs e)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("C:\\Users\\фвьшт\\Downloads\\uniformImageWindow.png");
            bitmapImage.DecodePixelWidth = 51;
            bitmapImage.DecodePixelHeight = 51;
            bitmapImage.EndInit();

            image.Source = bitmapImage;
            new HistogramsWindow(image).Show();
        }

        private void nonuniformHistogramButton_Click(object sender, RoutedEventArgs e)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("C:\\Users\\фвьшт\\Downloads\\nonuniformImageWindow.png");
            bitmapImage.DecodePixelWidth = 51;
            bitmapImage.DecodePixelHeight = 51;
            bitmapImage.EndInit();

            image.Source = bitmapImage;
            new HistogramsWindow(image).Show();
        }
    }
}
