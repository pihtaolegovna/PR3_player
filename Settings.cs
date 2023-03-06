using Newtonsoft.Json;
using PR3_player.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PR3_player
{
    public class Settings : MainWindow
    {

        public static bool DarkTheme;
        public static Brush Color { get; set; }
        public static bool Repeat;
        public static bool Shuffle;
        public static string OpenedDirectory;
        public static int Color_s; // BorderBrush вменяемо не получилось конвертировать из string. Так что сохраняю число, а потом конвертирую индекс в цвет
        public static int colorcount;




        public static void Save()
        {
            // Цвет в число
            if (Color == Brushes.Violet) Color_s = 1;
            if (Color == Brushes.GreenYellow) Color_s = 2;
            if (Color == Brushes.White) Color_s = 3;
            if (Color == Brushes.Black) Color_s = 3; // Храним один и тот же индекс, все равно потом перепроврять
            if (Color == Brushes.Red) Color_s = 4;
            if (Color == Brushes.Blue) Color_s = 5;
            if (Color == Brushes.Yellow) Color_s = 6;
            if (Color == Brushes.Green) Color_s = 7;
            if (Color == Brushes.Navy) Color_s = 8;

            var settings = new
            {
                DarkTheme,
                Color_s,
                OpenedDirectory
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("D:/ProgramData/Dno_player/settings.json", json);
        }

        public static void Load()
        {
            if (File.Exists("D:/ProgramData/Dno_player/settings.json"))
            {
                var json = File.ReadAllText("D:/ProgramData/Dno_player/settings.json");
                var settings = JsonConvert.DeserializeObject<dynamic>(json);
                DarkTheme = settings.DarkTheme;
                Color_s = settings.Color_s;
                colorcount = Color_s;
                switch (Color_s - 1)
                {
                    case 1: Color = Brushes.Violet; break;
                    case 2: Color = Brushes.GreenYellow; break;
                    case 3:
                        {
                            if (DarkTheme) Color = Brushes.White;
                            else Color = Brushes.Black;
                            break;
                        }
                    case 4: Color = Brushes.Red; break;
                    case 5: Color = Brushes.Blue; break;
                    case 6: Color = Brushes.Yellow; break;
                    case 7: Color = Brushes.Green; break;
                    case 8: Color = Brushes.Navy; break;
                }
                Repeat = false;
                Shuffle = false;
                OpenedDirectory = settings.OpenedDirectory;
            }
            else makeifnot();


        }

        public static Brush ChangeColor(bool change)
        {
            if (change)
            {
                colorcount+=1;
                if (colorcount > 8) colorcount = 1;
            }

            switch (colorcount)
            {
                case 1: Color = Brushes.Violet; break;
                case 2: Color = Brushes.GreenYellow; break;
                case 3:
                    {
                        if (DarkTheme) Color = Brushes.White;
                        else Color = Brushes.Black;
                        break;
                    }
                case 4: Color = Brushes.Red; break;
                case 5: Color = Brushes.Blue; break;
                case 6: Color = Brushes.Yellow; break;
                case 7: Color = Brushes.Green; break;
                case 8: Color = Brushes.Navy; break;
            }


            return Color;
        }

        public static Brush ChangeTheme(bool change)
        {
            Brush Background;

            if (DarkTheme)
            {
                if (change)
                {
                    Background = Brushes.BlanchedAlmond;
                    DarkTheme = false;
                }
                else Background = Brushes.Black;

            }
            else
            {
                if (change)
                {
                    Background = Brushes.Black;
                    DarkTheme = true;
                }
                else Background = Brushes.BlanchedAlmond;
            }


            return Background;


        }

        public static void makeifnot()
        {
            Directory.CreateDirectory("D:/ProgramData/Dno_player/");
            FileStream fileStream = System.IO.File.Create("D:/ProgramData/Dno_player/settings.json");
            fileStream.Dispose();

            DarkTheme = true;
            Color = Brushes.Red;
            Repeat = false;
            Shuffle = false;
            OpenedDirectory = "l";

            Save();
            Load(); // тут же пишем и читаем
        }
    }
}