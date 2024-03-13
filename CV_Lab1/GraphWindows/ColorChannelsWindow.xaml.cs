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
    /// Логика взаимодействия для ColorChannelsWindow.xaml
    /// </summary>
    public partial class ColorChannelsWindow : Window
    {
        private BitmapSource redBitmap;
        private BitmapSource greenBitmap;
        private BitmapSource blueBitmap;

        public ColorChannelsWindow(Image userImg)
        {
            InitializeComponent();
            DisplayColorChannels((BitmapSource)userImg.Source);
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

            this.redBitmap = redBitmap;
            this.greenBitmap = greenBitmap;
            this.blueBitmap = blueBitmap;
        }

        private void imgHistogramBtn_Click(object sender, RoutedEventArgs e)
        {
            if (redBitmap == null || greenBitmap == null || blueBitmap == null)
                return;
           // new HistogramsWindow(redBitmap, greenBitmap, blueBitmap).Show();
        }
    }
}
