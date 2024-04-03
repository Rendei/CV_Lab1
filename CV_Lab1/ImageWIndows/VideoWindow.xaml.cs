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
using System.Windows.Threading;
using static CV_Lab1.Functions.EdgeDetectionFunctions;
using static CV_Lab1.Functions.ImageFunctions;

namespace CV_Lab1.ImageWIndows
{
    /// <summary>
    /// Логика взаимодействия для VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        private Uri videoSource;
        private DispatcherTimer timer;
        public VideoWindow(Uri source)
        {
            InitializeComponent();
            videoPlayer.Source = source;
            videoSource = source;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            videoPlayer.Source = videoSource;
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;
        }

        //void timer_Tick(object sender, EventArgs e)
        //{
        //    if (videoPlayer.Source != null)
        //    {
        //        if (videoPlayer.NaturalDuration.HasTimeSpan)
        //            lblStatus.Content = String.Format("{0} / {1}", videoPlayer.Position.ToString(@"mm\:ss"), videoPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
        //    }
        //    else
        //        lblStatus.Content = "Не выбрано видео...";
        //}

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(1.0 / 120); // frame rate here
            timer.Tick += Timer_Tick;

            timer.Start();
            videoPlayer.Play();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                600,
                300,
                96,
                96,
                PixelFormats.Pbgra32);

            renderTargetBitmap.Render(videoPlayer);


            BitmapSource bitmapSource = ApplyLoGVideo(renderTargetBitmap, 3, 1);
            bitmapSource = GetNegativeImage(bitmapSource);

            image.Source = bitmapSource;

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
                timer.Start();
            videoPlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            videoPlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Stop();

        }
    }
}
