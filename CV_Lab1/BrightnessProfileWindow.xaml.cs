using LiveCharts;
using LiveCharts.Wpf;
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
    /// Логика взаимодействия для BrightnessProfileWindow.xaml
    /// </summary>
    public partial class BrightnessProfileWindow : Window
    {
        private Image userImg;

        public SeriesCollection BrightnessProfile { get; set; }
        public List<string> Labels { get; set; }

        public BrightnessProfileWindow(Image userImg)
        {
            InitializeComponent();

            BrightnessProfile = new SeriesCollection();
            Labels = new List<string>();

            // Set DataContext
            DataContext = this;
            this.userImg = userImg;
        }

        private void ShowBrightnessProfile(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            int chosenRow;
            if (!int.TryParse(RowTextBox.Text, out chosenRow) || chosenRow < 0 || chosenRow >= userImg.Source.Height)
            {
                MessageBox.Show($"Пожалуйста введите число до {userImg.Source.Height - 1}");
                return;
            }

            int width = ((BitmapSource)userImg.Source).PixelWidth;

            byte[] pixels = new byte[width * 4];
            ((BitmapSource)userImg.Source).CopyPixels(new Int32Rect(0, chosenRow, width, 1), pixels, width * 4, 0);

            ChartValues<double> brightnessValues = new ChartValues<double>();
            for (int i = 0; i < width * 4; i += 4)
            {
                byte blue = pixels[i];
                byte green = pixels[i + 1];
                byte red = pixels[i + 2];

                double brightness =  (red + green + blue) / 3;
                brightnessValues.Add(brightness);
                Labels.Add((i / 4).ToString());
            }

            BrightnessProfile.Clear();
            BrightnessProfile.Add(new LineSeries
            {                
                Title = $"Y",
                Values = brightnessValues
            });
        }    
    }
}
