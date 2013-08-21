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



        private void ClickedBrowse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            grid.ItemsSource = null;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath).Where(f => new[] { ".mp3", ".wma" }.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();
                var tagFiles = LoadTagFiles(files);


                var editInfos = tagFiles.Select(s => new EditInfo
                {
                    File = s,
                    FileName = System.IO.Path.GetFileName(s.Name),
                    Album = s.Tag.Album,
                    Title = s.Tag.Title,
                    Artist = s.Tag.FirstPerformer,
                    CoArtist = s.Tag.Performers.Length > 1 ? s.Tag.Performers[1] : null,
                    Track = s.Tag.Track,
                    Year = s.Tag.Year,
                    Genre = s.Tag.FirstGenre

                }).ToList();
                grid.ItemsSource = editInfos;
                saveButton.IsEnabled = true;
            }
            else
            {
                saveButton.IsEnabled = false;
            }
        }

        private List<TagLib.File> LoadTagFiles(string[] files)
        {
            List<TagLib.File> tagFiles = new List<TagLib.File>();
            List<string> errors = new List<string>();
            foreach (var path in files)
            {
                try
                {
                    TagLib.File f = TagLib.File.Create(path);
                    tagFiles.Add(f);
                }
                catch (Exception ex)
                {
                    errors.Add(System.IO.Path.GetFileName(path));
                }
            }
            if (errors.Any())
            {
                System.Windows.MessageBox.Show("Corrupt files detected (These will not be loaded, manual entry required):\n\n" + string.Join("\n", errors), "Corrupt files in folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return tagFiles;
        }

        private void ClickedSave(object sender, RoutedEventArgs e)
        {
            var items = (List<EditInfo>)grid.ItemsSource;
            string targetDirectory = null;
            if (items.Count > 0)
            {
                var directory = System.IO.Path.GetDirectoryName(items.First().File.Name);
                targetDirectory = directory + "\\NamedFiles";
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory, true);
                }
                Directory.CreateDirectory(targetDirectory);
            }

            foreach (var item in items)
            {
                UpdateTags(item);
                if (!string.IsNullOrWhiteSpace(item.Title) && item.Track.HasValue)
                {
                    File.Copy(item.File.Name, targetDirectory + "\\" + item.Track.Value.ToString() + " " + item.Title + System.IO.Path.GetExtension(item.File.Name));
                }
            }
            System.Windows.MessageBox.Show("Saved metadata for media files", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateTags(EditInfo item)
        {
            item.File.Tag.Album = item.Album;
            item.File.Tag.Title = item.Title;
            AddArtists(item);
            if (item.Track.HasValue)
            {
                item.File.Tag.Track = item.Track.Value;
            }
            if (item.Year.HasValue)
            {
                item.File.Tag.Year = item.Year.Value;
            }
            if (!string.IsNullOrWhiteSpace(item.Genre))
            {
                item.File.Tag.Genres = new string[1] { item.Genre };
            }
            item.File.Save();
        }

        private void AddArtists(EditInfo item)
        {
            List<string> artists = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.Artist))
            {
                artists.Add(item.Artist);
            }
            if (!string.IsNullOrWhiteSpace(item.CoArtist))
            {
                artists.Add(item.CoArtist);
            }
            item.File.Tag.Performers = artists.ToArray();
        }
    }

    public class EditInfo
    {
        public TagLib.File File { get; set; }
        public string FileName { get; set; }
        public string Artist { get; set; }
        public string CoArtist { get; set; }
        public string Genre { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public uint? Track { get; set; }
        public uint? Year { get; set; }
    }
}
