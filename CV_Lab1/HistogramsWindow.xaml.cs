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
    /// Логика взаимодействия для HistogramsWindow.xaml
    /// </summary>
    public partial class HistogramsWindow : Window
    {
        public SeriesCollection RedSeriesCollection { get; set; }
        public SeriesCollection GreenSeriesCollection { get; set; }
        public SeriesCollection BlueSeriesCollection { get; set; }
        public int MaxValueRed { get; set; }
        public string[] Labels { get; set; }

        public HistogramsWindow(BitmapSource redBitmap, BitmapSource greenBitmap, BitmapSource blueBitmap)
        {
            InitializeComponent();

            RedSeriesCollection = new SeriesCollection();
            GreenSeriesCollection = new SeriesCollection();
            BlueSeriesCollection = new SeriesCollection();


            DrawHistogram(RedSeriesCollection, redBitmap);
            DrawHistogram(GreenSeriesCollection, greenBitmap);
            DrawHistogram(BlueSeriesCollection, blueBitmap);

            MaxValueRed = RedSeriesCollection.SelectMany(series => (ChartValues<int>)series.Values).Max();
            Labels = Enumerable.Range(0, 256).Select(x => x.ToString()).ToArray();
            DataContext = this;
        }

        void DrawHistogram(SeriesCollection seriesCollection, BitmapSource channelBitmap)
        {
            int width = channelBitmap.PixelWidth;
            int height = channelBitmap.PixelHeight;

            byte[] pixels = new byte[width * height];
            channelBitmap.CopyPixels(pixels, width, 0);

            List<int> histogramData = new List<int>(Enumerable.Repeat(0, 256));

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte pixelValue = pixels[i];

                histogramData[pixelValue]++;
            }

            seriesCollection.Add(new ColumnSeries
            {
                Values = new ChartValues<int>(histogramData)
            });
        }
    }
}
