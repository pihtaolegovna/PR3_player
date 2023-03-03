using Newtonsoft.Json;
using PR3_player.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PR3_player
{
    public class Settings
    {

        public static bool DarkTheme;
        public static Brush Color { get; set;  }
        public static bool Repeat;
        public static bool Shuffle;
        public static string OpenedDirectory;

        public static int colorcount;


        public static void Save()
        {
            existcheck();
            var settings = new
            {
                DarkTheme,
                Color = Color.ToString(),
                Repeat,
                Shuffle,
                OpenedDirectory
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("D:/ProgramData/Dno_player/settings.json", json);
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
                    Background = Brushes.White;
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
                else Background = Brushes.White;
            }


            return Background;


        }

        public static void Load()
        {
            if (File.Exists("D:/ProgramData/Dno_player/settings.json"))
            {
                var json = File.ReadAllText("D:/ProgramData/Dno_player/settings.json");
                var settings = JsonConvert.DeserializeObject<dynamic>(json);

                DarkTheme = settings.DarkTheme;

                // Color = (Brush)new BrushConverter().ConvertFromString(settings.Color);

                Color = Brushes.Red;
                Repeat = settings.Repeat;
                Shuffle = settings.Shuffle;
                OpenedDirectory = settings.OpenedDirectory;
            }
            else existcheck();

            
        }
        public static void existcheck()
        {
            if (!System.IO.File.Exists("D:/ProgramData/Dno_player/settings.json"))
            {
                Directory.CreateDirectory("D:/ProgramData/Dno_player/");
                FileStream fileStream = System.IO.File.Create("D:/ProgramData/Dno_player/settings.json");
                fileStream.Dispose();

                Settings.DarkTheme = true;
                Settings.Color = Brushes.Red;
                Settings.Repeat = false;
                Settings.Shuffle = true;
                Settings.OpenedDirectory = "";
            }
        }
    }
}
