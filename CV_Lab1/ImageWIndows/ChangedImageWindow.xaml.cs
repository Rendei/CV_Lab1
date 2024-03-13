using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для ChangedImageWindow.xaml
    /// </summary>
    public partial class ChangedImageWindow : Window
    {
        public Image changedImg { get; set; }
        public ChangedImageWindow(Image changedImg)
        {
            InitializeComponent();
            this.changedImg = changedImg;
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {              
                DefaultExt = ".png",
            };
            
            bool? isSaving = saveFileDialog.ShowDialog();
            if (isSaving == true)
            {
                using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)changedImg.Source));
                    encoder.Save(stream);
                }

                MessageBox.Show($"Файл успешно сохранён с названием {saveFileDialog.SafeFileName}");
            }
            
        }
    }
}
