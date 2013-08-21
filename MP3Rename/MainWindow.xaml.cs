using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace MP3Rename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<TagLib.File> tagFiles;

        private void ClickedBrowse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            grid.ItemsSource = null;
            tagFiles = new List<TagLib.File>();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath).Where(f => new[] { ".mp3", ".wma" }.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();


                foreach (var path in files)
                {
                    TagLib.File f = TagLib.File.Create(path);
                    tagFiles.Add(f);


                }
                var editInfos = tagFiles.Select(s => new EditInfo
                {
                    File = s,
                    Album = s.Tag.Album,
                    Title = s.Tag.Title,
                    Artist = s.Tag.FirstAlbumArtist,
                    Track = s.Tag.Track,
                    Year = s.Tag.Year

                }).ToList();
                grid.ItemsSource = editInfos;
                saveButton.IsEnabled = true;
            }
            else
            {
                saveButton.IsEnabled = false;
            }
        }

        private void ClickedSave(object sender, RoutedEventArgs e)
        {
            foreach (var item in (List<EditInfo>)grid.ItemsSource)
            {
                item.File.Tag.Album = item.Album;
                item.File.Tag.Title = item.Title;
                item.File.Tag.AlbumArtists[0] = item.Artist;
                item.File.Tag.Track = item.Track;
                item.File.Tag.Year = item.Year;
                item.File.Save();
            }
            System.Windows.MessageBox.Show("Saved metadata for mp3 files");
        }
    }

    public class EditInfo
    {
        public TagLib.File File { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public uint Track { get; set; }
        public uint Year { get; set; }
    }
}
