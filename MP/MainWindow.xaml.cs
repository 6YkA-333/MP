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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using TagLib;

namespace MP
{

        public partial class MainWindow : Window
        {
            public DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

        timelineSlider.Maximum = 100;
        InitializeTimer();
        StartBackgroundAnimation();

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
            {
            mediaPlayer.Play();
            statusText.Text = "Playing";
            timer.Start();
        }

            private void PauseButton_Click(object sender, RoutedEventArgs e)
            {
            mediaPlayer.Pause();
            statusText.Text = "Paused";
            timer.Stop();
        }

            private void StopButton_Click(object sender, RoutedEventArgs e)
            {
                mediaPlayer.Stop();
                statusText.Text = "Stopped";
            }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSpan duration = mediaPlayer.NaturalDuration.TimeSpan;
            statusText.Text = $"Now Playing: {System.IO.Path.GetFileName(mediaPlayer.Source.ToString())} - {duration.Minutes}:{duration.Seconds}";
            UpdateTimelineSlider();
            timer.Start();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 Files|*.mp3|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName);
                statusText.Text = "File Loaded";
                UpdateTrackInfo(openFileDialog.FileName);
                LoadAlbumCover(openFileDialog.FileName);
            }
        }
        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan newPosition = TimeSpan.FromSeconds(e.NewValue);
            mediaPlayer.Position = newPosition;
        }

        private void UpdateTimelineSlider()
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                double totalSeconds = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                timelineSlider.Maximum = totalSeconds;
                timelineSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimelineSlider();
        }




        private void LoadAlbumCover(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                var tag = file.Tag;

                if (tag.Pictures.Length > 0)
                {
                    var imageData = tag.Pictures[0].Data.Data;
                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(imageData);
                    bitmapImage.EndInit();

                    coverImage.Source = bitmapImage;

                }
                else
                {
                    // Если обложки нет, можно установить стандартное изображение
                    coverImage.Source = new BitmapImage(new Uri("pack://application:,,,/YourNamespace;component/Resources/DefaultCover.jpg"));
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке обложки
                MessageBox.Show($"У этого трека отсутствует оболожка", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                coverImage.Source = null;
            }
        }

        private void StartBackgroundAnimation()
        {
            try
            {
                ColorAnimation colorAnimation = new ColorAnimation();
                colorAnimation.From = ((LinearGradientBrush)mainGrid.Background).GradientStops[0].Color;
                colorAnimation.To = ((LinearGradientBrush)mainGrid.Background).GradientStops[1].Color;
                colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(5)); // Продолжительность анимации в секундах
                colorAnimation.AutoReverse = true; // Добавляет эффект возврата к исходному цвету

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(colorAnimation);

                Storyboard.SetTarget(colorAnimation, mainGrid.Background);
                Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("GradientStops[0].Color"));

                storyboard.Begin();
            }
            catch { }
        }
        private void UpdateTrackInfo(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                var tag = file.Tag;

                infoTrack.Text = $"Название: {tag.Title}\n" +
                                $"Артист: {tag.FirstPerformer}\n" +
                                $"Албьом: {tag.Album}\n" +
                                $"Год выпуска: {tag.Year.ToString()}\n" +
                                $"Жанр: {tag.FirstGenre}\n" +
                                $"Длительность: {file.Properties.Duration:mm\\:ss}";
                elapsedTimeText.Text = $"{file.Properties.Duration:mm\\:ss}";
            }
            catch (Exception ex)
            {
                // Обработка ошибок при чтении метаданных
                MessageBox.Show($"Error reading track information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
    }
