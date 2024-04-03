﻿using System;
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

namespace CV_Lab1.ImageWIndows
{
    /// <summary>
    /// Логика взаимодействия для AntialiasingPreviewWindow.xaml
    /// </summary>
    public partial class AntialiasingPreviewWindow : Window
    {
        public AntialiasingPreviewWindow(Image image)
        {
            InitializeComponent();
            differenceMapImage.Source = image.Source;
        }
    }
}