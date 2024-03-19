using CV_Lab1.Functions;
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
using static CV_Lab1.Functions.ImageFunctions;
using static CV_Lab1.Functions.ChromaticityFunctions;
using static CV_Lab1.Functions.SmoothingFunctions;
using System.Text.RegularExpressions;

namespace CV_Lab1
{
    /// <summary>
    /// Логика взаимодействия для ImageChangerWindow.xaml
    /// </summary>
    public partial class ImageChangerWindow : Window
    {
        private enum NeigbourAmount
        {
            Four = 4,
            Eight = 8,
        }
        private Image userImg;
        private Image changedImg;
        private Image tmpChangedImg = new Image();

        private long _contactNo;
        public long contactNo
        {
            get { return _contactNo; }
            set
            {
                if (value == _contactNo)
                    return;
                _contactNo = value;
                OnPropertyChanged(new DependencyPropertyChangedEventArgs());
            }
        }

        public ImageChangerWindow(Image userImg, Image changedImg)
        {
            InitializeComponent();
            this.userImg = userImg;
            this.changedImg = changedImg;
            this.tmpChangedImg.Source = changedImg.Source;
            redChannelRadioButton.IsChecked = true;
            fourNeighbourButton.IsChecked = true;

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
            
            BitmapSource originalSource = (BitmapSource)changedImg.Source; //!!!
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
            changedImg.Source = adjustedImage;
            tmpChangedImg.Source = changedImg.Source;
        }

        private void UpdateContrast()
        {
            if (userImg.Source == null)
                return;
            
            BitmapSource originalSource = (BitmapSource)tmpChangedImg.Source; //!!!
            originalSource = new FormatConvertedBitmap(originalSource, PixelFormats.Bgra32, null, 0);
            double threshold = contrastSlider.Value;
            BitmapSource adjustedImage = AdjustContrast(originalSource, threshold);
            changedImg.Source = adjustedImage;
            
        }

        private void negativeImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;


            changedImg.Source = GetNegativeImage((BitmapSource)changedImg.Source); //!!
            tmpChangedImg.Source = changedImg.Source;
        }

        private void changeChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = SwapColorChannels((BitmapSource)changedImg.Source,  //!!
                int.Parse(((ComboBoxItem)firstChannelComboBox.SelectedItem).Tag.ToString()), 
                int.Parse(((ComboBoxItem)secondChannelComboBox.SelectedItem).Tag.ToString()));
            tmpChangedImg.Source = changedImg.Source;
        }

        private void flipImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = FlipImageVertically((BitmapSource)changedImg.Source); //!!!
            tmpChangedImg.Source = changedImg.Source;
        }

        private void removeNoiseImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            RadioButton checkedRadioButton = this.noisePanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);

            if (checkedRadioButton == null)
                return;

            NeigbourAmount neigbourAmount;
            switch (checkedRadioButton.Name)
            {
                case "fourNeighbourButton":
                    neigbourAmount = NeigbourAmount.Four;
                    break;
                case "eightNeighbourButton":
                    neigbourAmount = NeigbourAmount.Eight;
                    break;
                default:
                    return;
            }

            changedImg.Source = RemoveNoise((BitmapSource)changedImg.Source, (int)neigbourAmount); //!!!
            tmpChangedImg.Source = changedImg.Source;
        }


        private void originalImageButton_Click(object sender, RoutedEventArgs e)
        {
            changedImg.Source = userImg.Source;
            channelBrightnessSlider.Value = 0;
            contrastSlider.Value = 0;
            
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            channelBrightnessSlider.Value = 0;
        }

        private void scaleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = NormalizePixelValues((BitmapSource)changedImg.Source);
            tmpChangedImg.Source = changedImg.Source;
        }

        private void logarithmicTransformImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = ApplyLogarithmicTransform((BitmapSource)changedImg.Source);
            tmpChangedImg.Source = changedImg.Source;
        }

        private void exponentionalTransformImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            try
            {
                double exponent = double.Parse(exponentTextBox.Text);
                changedImg.Source = ApplyExponentialTransform((BitmapSource)changedImg.Source, exponent);
                tmpChangedImg.Source = changedImg.Source;
            }
            catch
            {
                MessageBox.Show("Введите верное значение степени!");
            }
            
        }        

        private void binaryTransformImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            try
            {
                byte threshold = byte.Parse(binaryTextBox.Text);
                changedImg.Source = ApplyBinaryTransform((BitmapSource)changedImg.Source, threshold);
                tmpChangedImg.Source = changedImg.Source;
            }
            catch
            {
                MessageBox.Show("Введите значение порога до 255!");
            }
            
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void cutTransformImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;
            byte minValue;
            byte maxValue;

            if (byte.TryParse(cutMinTextBox.Text, out minValue) && byte.TryParse(cutMaxTextBox.Text, out maxValue))
            {
                if (minValue < maxValue)
                {
                    var dialogResult = MessageBox.Show("Привести пиксели вне диапазона к константным значениям?", "Перевод пикселей", MessageBoxButton.YesNo);

                    if (dialogResult == MessageBoxResult.No)
                    {
                        changedImg.Source = CutPixelRange((BitmapSource)changedImg.Source, minValue, maxValue, keepOriginal: false);
                        tmpChangedImg.Source = changedImg.Source;
                    }
                    else
                    {
                        changedImg.Source = CutPixelRange((BitmapSource)changedImg.Source, minValue, maxValue, 50, true);
                        tmpChangedImg.Source = changedImg.Source;
                    }
                }
                else
                {
                    MessageBox.Show("Минимальное значение должно быть меньше максимального!");
                }
            }
            else
            {
                MessageBox.Show("Введите значение минимума и максимума до 255!");
            }
            
        }

        private void rectangularFilterImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = ApplyRectangularFilter((BitmapSource)changedImg.Source);
            tmpChangedImg.Source = changedImg.Source;
        }

        private void medianFilterImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            changedImg.Source = ApplyMedianFilter((BitmapSource)changedImg.Source);
            tmpChangedImg.Source = changedImg.Source;
        }

        private void gaussianFilterImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;
 
                double sigma = double.Parse(gaussianTextBox.Text);
                changedImg.Source = ApplyGaussianFilter((BitmapSource)changedImg.Source, sigma);
                tmpChangedImg.Source = changedImg.Source;                             
        }

        private void sigmaFilterImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (userImg.Source == null)
                return;

            double sigma = double.Parse(sigmaTextBox.Text);
            changedImg.Source = ApplySigmaFilter((BitmapSource)changedImg.Source, sigma);
            tmpChangedImg.Source = changedImg.Source;
        }
    }
}
