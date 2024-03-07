using LiveCharts;
using LiveCharts.Defaults;
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
using static CV_Lab1.ImageFunctions;

namespace CV_Lab1
{
    /// <summary>
    /// Логика взаимодействия для ContrastMapWindow.xaml
    /// </summary>
    public partial class ContrastMapWindow : Window
    {

        private Image userImg;
        public SeriesCollection ContrastMap { get; set; }

        private enum NeighborType
        {
            FourNeighbors,
            EightNeighbors,
            ArbitraryWindow,
        }

        public ContrastMapWindow(Image userImg)
        {
            InitializeComponent();
            ContrastMap = new SeriesCollection();
            
            this.userImg = userImg;
            DataContext = this;
        }

        private void ShowContrastMap(object sender, RoutedEventArgs e)
        {
            NeighborType neighborType = NeighborType.FourNeighbors;
            if (EightNeighborsRadioButton.IsChecked == true)
            {
                neighborType = NeighborType.EightNeighbors;
            }
            else if (ArbitraryWindowRadioButton.IsChecked == true)
            {
                neighborType = NeighborType.ArbitraryWindow;
            }

            int width = ((BitmapSource)userImg.Source).PixelWidth;
            int height = ((BitmapSource)userImg.Source).PixelHeight;

            double[,] contrastMap = new double[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double contrast = CalculateContrast((BitmapSource)userImg.Source, x, y, neighborType);
                    contrastMap[x, y] = contrast;
                }
            }

            UpdateContrastMapChart(contrastMap, width, height);
        }

        private double CalculateContrast(BitmapSource image, int x, int y, NeighborType neighborType)
        {
            List<byte> neighborValues = new List<byte>();

            if (neighborType == NeighborType.FourNeighbors || neighborType == NeighborType.EightNeighbors)
            {
                if (x - 1 >= 0) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x - 1, y))); // Left
                if (x + 1 < image.PixelWidth) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x + 1, y))); // Right
                if (y - 1 >= 0) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x, y - 1))); // Top
                if (y + 1 < image.PixelHeight) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x, y + 1))); // Bottom
            }

            if (neighborType == NeighborType.EightNeighbors)
            {
                if (x - 1 >= 0 && y - 1 >= 0) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x - 1, y - 1))); // Top-left
                if (x + 1 < image.PixelWidth && y - 1 >= 0) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x + 1, y - 1))); // Top-right
                if (x - 1 >= 0 && y + 1 < image.PixelHeight) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x - 1, y + 1))); // Bottom-left
                if (x + 1 < image.PixelWidth && y + 1 < image.PixelHeight) neighborValues.Add(GetAverageIntensity(GetPixelColor(image, x + 1, y + 1))); // Bottom-right
            }

            if (neighborType == NeighborType.ArbitraryWindow)
            {
                int windowSize = 51;
                int halfWindowSize = windowSize / 2;

                for (int i = -halfWindowSize; i <= halfWindowSize; i++)
                {
                    for (int j = -halfWindowSize; j <= halfWindowSize; j++)
                    {
                        int neighborX = x + i;
                        int neighborY = y + j;

                        if (neighborX >= 0 && neighborX < image.PixelWidth && neighborY >= 0 && neighborY < image.PixelHeight)
                        {
                            neighborValues.Add(GetAverageIntensity(GetPixelColor(image, neighborX, neighborY)));
                        }
                    }
                }
            }

            byte minIntensity = neighborValues.Count > 0 ? neighborValues.Min() : GetAverageIntensity(GetPixelColor(image, x, y));
            byte maxIntensity = neighborValues.Count > 0 ? neighborValues.Max() : GetAverageIntensity(GetPixelColor(image, x, y));
            double contrast = maxIntensity - minIntensity;

            return contrast;
        }

        private byte GetAverageIntensity(Color color)
        {
            return (byte)((color.R + color.G + color.B) / 3);
        }

        private void UpdateContrastMapChart(double[,] contrastMap, int width, int height)
        {
            ChartValues<HeatPoint> heatPoints = new ChartValues<HeatPoint>();

            for (int y = 0; y < height / 3; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    heatPoints.Add(new HeatPoint(x, y, contrastMap[x, y]));
                }
            }

            ContrastMap.Clear();
            ContrastMap.Add(new HeatSeries
            {
                Values = heatPoints,
                DataLabels = false
            });
        }
    }
}
