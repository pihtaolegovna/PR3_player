using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

using System.Windows.Media.Imaging;
using System.Windows.Data;
using TagLib.Asf;

namespace PR3_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MediaPlayer mediaPlayer = new MediaPlayer();
        static int songpathindex = 0;
        static bool NowPlaying;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;

            Settings.Load();
            Binding binding = new Binding();


            setview(false, false);
            openfolder(Settings.OpenedDirectory);


        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.Save();
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void Theme_Changer(object sender, RoutedEventArgs e)
        {
            setview(false, true);
            
        }

        private void FolderOpening(object sender, RoutedEventArgs e)
        {

            openfolder("");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            search();
        }

        public void search()
        {
            string searchQuery = Finder.Text.ToLower();
            var searchResults = audio.allnames
                    .Where(item => item.ToLower().Contains(searchQuery))
                    .ToList();
            audio.foundeditems = audio.allabsolutepaths
                    .Where(item => item.ToLower().Contains(searchQuery))
                    .ToList();

            audioListBox.ItemsSource = searchResults;
        }

        private void changeColor(object sender, RoutedEventArgs e)
        {
            setview(true, false);

        }
        private void setview(bool colorchange, bool themechange)
        {
            var NowBackground = Settings.ChangeTheme(themechange);
            var NowColor = Settings.ChangeColor(colorchange);

            Finder.Text = "Поиск";
            parentContainer.Background = NowBackground;
            header.Foreground = NowColor;

            foreach (var ListBox in parentContainer.Children.OfType<ListBox>())
            {
                ListBox.Foreground = NowColor;
            }

            foreach (var TextBox in parentContainer.Children.OfType<TextBox>())
            {
                TextBox.Foreground = NowColor;
                TextBox.Background = NowBackground;
            }

            foreach (var TextBlock in parentContainer.Children.OfType<TextBlock>())
            {
                TextBlock.Foreground = NowColor;
            }

            foreach (var button in parentContainer.Children.OfType<Button>())
            {
                button.Background = NowBackground;
                button.Foreground = NowColor;
                button.BorderBrush = Brushes.Transparent;
            }
            foreach (var ListBox in AppArea.Children.OfType<ListBox>())
            {
                ListBox.Foreground = NowColor;
            }

            foreach (var TextBox in AppArea.Children.OfType<TextBox>())
            {
                TextBox.Foreground = NowColor;
                TextBox.Background = NowBackground;
            }

            foreach (var TextBlock in AppArea.Children.OfType<TextBlock>())
            {
                TextBlock.Foreground = NowColor;
            }

            foreach (var button in AppArea.Children.OfType<Button>())
            {
                button.Background = NowBackground;
                button.Foreground = NowColor;
                button.BorderBrush = Brushes.Transparent;
            }

        }
        private void openfolder(string path)
        {
            if (path == "")
            {
                // Открытие диалога выбора папки
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true
                };
                dialog.Title = "Выберите папку с музыкой";

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    // Список расширений файлов, которые мы хотим прочитать
                    var fileExtensions = new List<string> { ".mp3", ".wav", ".flac" };

                    // Чтение файлов из выбранной папки с заданными расширениями
                    foreach (string file in Directory.GetFiles(dialog.FileName))
                    {
                        if (fileExtensions.Contains(Path.GetExtension(file)))
                        {

                            audio.all.Add(new audio(Path.GetFullPath(file)));


                        }
                    }
                    Settings.OpenedDirectory = dialog.FileName;



                    foreach (audio item in audio.all)
                    {
                        audio.allnames.Add(item.FileName);
                    }
                    audioListBox.ItemsSource = audio.allnames;
                }
            }
            else
            {
                // Список расширений файлов, которые мы хотим прочитать
                var fileExtensions = new List<string> { ".mp3", ".wav", ".flac" };

                // Чтение файлов из выбранной папки с заданными расширениями
                foreach (string file in Directory.GetFiles(path))
                {
                    if (fileExtensions.Contains(Path.GetExtension(file)))
                    {

                        audio.all.Add(new audio(Path.GetFullPath(file)));


                    }
                }



                foreach (audio item in audio.all)
                {
                    audio.allnames.Add(item.FileName);
                }
                foreach (audio item in audio.all)
                {
                    audio.allabsolutepaths.Add(item.FilePath);
                }
                audioListBox.ItemsSource = audio.allnames;
            }
            ine(audio.allabsolutepaths[songpathindex]);
        }


        public void ine(string path)
        {
            mediaPlayer.Open(new Uri(path, UriKind.Absolute));

            foreach (audio item in audio.all)
            {
                if (item.FilePath == path)
                {
                    SongName.Text = item.FileName;
                    ArtistName.Text = item.Artist ?? "Unknown Artist";
                    AlbumName.Text = item.Album ?? "Unknown Album";
                    Year.Text = item.Year.ToString() ?? "Unknown Year";
                    if (item.Cover.Length > 0)
                    {
                        var picture = item.Cover[0];
                        using (var stream = new MemoryStream(picture.Data.Data))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            SongImage.Source = bitmap;
                        }
                    }
                    else
                    {
                        SongImage.Source = null;
                    }


                }
                
            }

            // устанавливаем максимальное значение для Slider
            mediaPlayer.Play();

            // задаем обработчик события для перемотки

            
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ine(audio.allabsolutepaths[songpathindex += 1]);
            }
            catch
            {
                ine(audio.allabsolutepaths[songpathindex -= 1]);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ine(audio.allabsolutepaths[songpathindex += 1]);
            }
            catch
            {
                ine(audio.allabsolutepaths[songpathindex -= 1]);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlaying)
            {
                mediaPlayer.Play();
                NowPlaying = false;
                Pause.Content = Char.ConvertFromUtf32(0xE769);
            }
            else
            {
                mediaPlayer.Pause();
                NowPlaying = true;
                Pause.Content = Char.ConvertFromUtf32(0xE768);
            }
            
        }
        

        private void audioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ine(audio.foundeditems[audioListBox.SelectedIndex]);
            }
            catch
            {
                // Когда список пустой при поиске, не пытаемся ничего запустить. Корень проблемы при запуске без поиска
                if (audioListBox.SelectedIndex > -1) ine(audio.allabsolutepaths[audioListBox.SelectedIndex]);

            }
        }

        private void Finder_GotFocus(object sender, RoutedEventArgs e)
        {
            Finder.Text = null;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = new TimeSpan(0, 0, (int)e.NewValue);
            
            mediaPlayer.Position = ts;
        }

        private void audioSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            mediaPlayer.Pause();
        }

        private void audioSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(slider.Value);
            mediaPlayer.Play();
        }

        private void audioSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(slider.Value);
            }
        }
        void mediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            slider.Maximum = Convert.ToInt64(mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
        }

        void mediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            slider.Value = 0;
        }

        //E78B - Search E96F E970 is arrows E768 E769 Play/Pause E8EE - Repeat ED25 - Foledr E790 - Color E793 - Sun
    }
}
