using System;
using System.Collections.Generic;
using System.IO;
using TagLib;

public class audio
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public TagLib.File File { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public uint Year { get; set; }
    public IPicture[] Cover { get; set; }

    public static List<audio> all = new List<audio>();

    public static List<string> allnames = new List<string>();
    public static List<string> allabsolutepaths = new List<string>();
    public static List<string> foundeditems = new List<string>();

    public audio(string filePath)
    {


        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        FilePath = filePath;
        FileName = Path.GetFileNameWithoutExtension(filePath);

        File = TagLib.File.Create(filePath);
        if (File.Tag != null)
        {
            Artist = File.Tag.FirstAlbumArtist;
            Album = File.Tag.Album;
            Year = File.Tag.Year;
            Cover = File.Tag.Pictures;
        }
        else
        {
            Artist = "Неизвестный исполнитель";
            Album = "Неизвестный Альбом";
            Year = 0000;
        }
    }
}
