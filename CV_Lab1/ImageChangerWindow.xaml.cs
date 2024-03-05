using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static CV_Lab1.ImageFunctions;

namespace CV_Lab1
{
    /// <summary>
    /// Логика взаимодействия для ImageChangerWindow.xaml
    /// </summary>
    public partial class ImageChangerWindow : Window
    {
        private Image userImg;

        public ImageChangerWindow(Image userImg)
        {
            InitializeComponent();
            this.userImg = userImg;
        }

        private void channelBrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBrightness();
        }

        private void contrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateContrast();
        }

        private void UpdateBrightness()
        {
            if (userImg.Source == null)
                return;

            // Get the original image source
            BitmapSource originalSource = (BitmapSource)userImg.Source;
            originalSource = new FormatConvertedBitmap(originalSource, PixelFormats.Bgra32, null, 0);

            // Get the selected color channel
            RadioButton checkedRadioButton = this.colorPanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);

            if (checkedRadioButton == null)
                return;

            ColorChannel selectedChannel;
            switch (checkedRadioButton.Name)
            {
                case "redChannelRadioButton":
                    selectedChannel = ColorChannel.Red;
                    break;
                case "greenChannelRadioButton":
                    selectedChannel = ColorChannel.Green;
                    break;
                case "blueChannelRadioButton":
                    selectedChannel = ColorChannel.Blue;
                    break;
                default:
                    return;
            }

            double brightnessValue = channelBrightnessSlider.Value;

            BitmapSource adjustedImage = AdjustBrightness(originalSource, selectedChannel, brightnessValue);
            userImg.Source = adjustedImage;
        }

        private void UpdateContrast()
        {
            if (userImg.Source == null)
                return;

            BitmapSource originalSource = (BitmapSource)userImg.Source;
            originalSource = new FormatConvertedBitmap(originalSource, PixelFormats.Bgra32, null, 0);
            double threshold = contrastSlider.Value;
            BitmapSource adjustedImage = AdjustContrast(originalSource, threshold);
            userImg.Source = adjustedImage;
            
        }

        private void negativeImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;


            userImg.Source = GetNegativeImage((BitmapSource)userImg.Source);
        }

        private void changeChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            userImg.Source = SwapColorChannels((BitmapSource)userImg.Source, 
                int.Parse(((ComboBoxItem)firstChannelComboBox.SelectedItem).Tag.ToString()), 
                int.Parse(((ComboBoxItem)secondChannelComboBox.SelectedItem).Tag.ToString()));
        }

        private void flipImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            userImg.Source = FlipImageVertically((BitmapSource)userImg.Source);
        }
    }
}
