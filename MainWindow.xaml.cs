﻿using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace PR3_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<int> AlreadyPlayedSongs = new List<int>();
        static int songpathindex = 0;
        static bool NowPlaying;
        static string songpath = "";
        TimeSpan timespan;
        static bool slidercapture;

        public MainWindow()
        {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;

            Settings.Load();


            setview(false, false);
            openfolder(Settings.OpenedDirectory);


        }

        private void Theme_Changer(object sender, RoutedEventArgs e)
        {
            setview(false, true);
            
        }
        private void FolderOpening(object sender, RoutedEventArgs e)
        {
            openfolder("l");
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Finder.Text != "Поиск") search();
        }
        private static TimeSpan TimeSpanExtractor(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                return TimeSpan.FromTicks((long)t);
            }
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
        public void setview(bool colorchange, bool themechange)
        {
            themechanger.Background = Settings.ChangeTheme(themechange);
            header.Foreground = Settings.ChangeColor(colorchange);

            if (!Settings.Shuffle) Shuffle.Foreground = Brushes.Gray;
            if (!Settings.Repeat) Shuffle.Foreground = Brushes.Gray;

        }
        private void openfolder(string path)
        {
            if (path == "l")
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
                    var fileExtensions = new List<string> { ".mp3", ".wav", ".flac", ".mp4" };

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
                        var ArtistNameText = item.Artist ?? "Unknown Artist";
                        var AlbumNameText = item.Album ?? "Unknown Album";
                        var YearText = item.Year.ToString() ?? "Unknown Year";
                        audio.allnames.Add(item.FileName + "\n" + ArtistNameText + "\n" + AlbumNameText + " × " + YearText);
                    }
                    audioListBox.ItemsSource = audio.allnames;
                    foreach (audio item in audio.all)
                    {
                        audio.allabsolutepaths.Add(item.FilePath);
                    }
                    audioListBox.ItemsSource = audio.allnames;
                }
            }
            else
            {
                // Список расширений файлов, которые мы хотим прочитать
                var fileExtensions = new List<string> { ".mp3", ".wav", ".flac", ".mp4" };

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
                    var ArtistNameText = item.Artist ?? "Unknown Artist";
                    var AlbumNameText = item.Album ?? "Unknown Album";
                    var YearText = item.Year.ToString() ?? "Unknown Year";
                    audio.allnames.Add(item.FileName + "\n" + ArtistNameText + "\n" + AlbumNameText + " × " + YearText);
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
            mediaPlayer.Source = new Uri(path, UriKind.Absolute);

            


            slider.Value = 0;


            foreach (audio item in audio.all)
            {
                if (item.FilePath == path)
                {
                    songpath = item.FilePath;
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



            if (!NowPlaying) // Если нажата пауза, при переключении песни мы не воспроизводим ее
            {
                mediaPlayer.Play();
            }


            timespan = TimeSpanExtractor(songpath);




            int seconds = Convert.ToInt32(timespan.TotalSeconds);
            int minutes = seconds / 60;
            seconds %= 60;
            EndTimer.Text = $"{minutes}:{(seconds < 10 ? $"0{seconds}" : $"{seconds}")}";

            progress.Maximum = Convert.ToInt64(timespan.Ticks);


            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0);
            dispatcherTimer.Start();
            NowTimer.Text = "0:00";


        }
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int seconds = Convert.ToInt32(mediaPlayer.Position.TotalSeconds);
            int minutes = seconds / 60;
            seconds %= 60;
            

            if (!slidercapture)
            {
                progress.Value = mediaPlayer.Position.Ticks;
                NowTimer.Text = $"{minutes}:{(seconds < 10 ? $"0{seconds}" : $"{seconds}")}";
            }
                
                

            


            CommandManager.InvalidateRequerySuggested();
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try { ine(audio.allabsolutepaths[songpathindex += 1]); }
            catch
            {
                try { ine(audio.allabsolutepaths[songpathindex += 1]); }
                catch { ine(audio.allabsolutepaths[songpathindex += 1]);}
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
            if (Finder.Text == "Поиск") Finder.Text = null; // Очищаем поиск от "Поиск" при нажатии, но при условии, что он никак не был задействован. Иначе список будет пустым

        }
        void mediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            slider.Value = 0;
            if (Settings.Repeat)
            {
                ine(songpath);
            }
            else PlayNextSong();
        }
        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            

            if (Settings.Shuffle) {
                Shuffle.Foreground = Brushes.Gray;
                Settings.Shuffle = false;
            }
            else
            {
                Shuffle.Foreground = Settings.Color;
                Settings.Shuffle = true;
            }
            AlreadyPlayedSongs.Clear();
        }
        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Repeat)
            {
                Repeat.Foreground = Brushes.Gray;
                Settings.Repeat= false;
                AlreadyPlayedSongs.Clear();
            }
            else
            {
                Repeat.Foreground = Settings.Color;
                Settings.Repeat = true;
            }
            
        }
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progress.Value = slider.Value;

            int seconds = Convert.ToInt32(Math.Round(slider.Value / 10000000));
            long minutes = seconds / 60;
            seconds %= 60;

            NowTimer.Text = $"{minutes}:{(seconds < 10 ? $"0{seconds}" : $"{seconds}")}";



            slidercapture = true;
            
        }
        private void mediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            mediaPlayer.Stop();
            if (!Settings.Repeat) PlayNextSong();
        }
        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            slidercapture = false; 
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                slider.Value = 0;
                slider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
            }
        }
        private void PlayNextSong() // не кнопка, а отдельный метод, поскольку вызывается аж 4 раза, в том числе рекурсией
        {
            if (Settings.Repeat)
            {
                ine(audio.allabsolutepaths[songpathindex]);
            }
            if (!Settings.Shuffle)
            {
                try
                {
                    ine(audio.allabsolutepaths[songpathindex += 1]); // Не позволяем listbox'у выйти за пределы значений
                }
                catch
                {
                    ine(audio.allabsolutepaths[0]);
                }
            }
            if (Settings.Shuffle)
            {
                Random randomsong = new Random();
                int nowsongindexnumber = -1;
                try
                {
                    nowsongindexnumber = randomsong.Next(0, audio.allabsolutepaths.Count);

                    int itemnumber = 0;
                    while (itemnumber < audio.allabsolutepaths.Count)
                    {
                        if (!AlreadyPlayedSongs.Contains(nowsongindexnumber))
                        {
                            AlreadyPlayedSongs.Add(nowsongindexnumber);
                            ine(audio.allabsolutepaths[nowsongindexnumber]);
                        }
                        itemnumber++;
                    }
                    AlreadyPlayedSongs.Clear();


                }
                catch
                {
                    PlayNextSong();
                }
                
                
            }

        }



        // Шапка то на WindowsChrome, поэтому еще пара страшных методов для вменяемой работы кнопок и шапки
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
        // Для закрытия
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.Save();
            SystemCommands.CloseWindow(this);
        }
        // Отвечает за смену кнопки во весь экран/Вид в окне
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void slider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(slider.Value));
            
            slidercapture = false;
        }

        private void slider_MouseEnter(object sender, MouseEventArgs e)
        {
            slidercapture = true;
            slider.Value = progress.Value;
        }

        private void slider_MouseLeave(object sender, MouseEventArgs e)
        {
            slidercapture = false;
        }
    }
}
