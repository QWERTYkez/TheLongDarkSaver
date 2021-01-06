using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TheLongDarkSaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectoryInfo BaseDir;
        DirectoryInfo SavesDir;
        bool AppLive = true;
        List<DirectoryInfo> Saves = new List<DirectoryInfo>();
        DateTime lwt;
        object getkey = new object();
        object setkey = new object();
        DateTime LWT 
        { 
            get 
            {
                lock (getkey)
                {
                    return lwt;
                }
            } 
            set 
            {
                lock (setkey)
                {
                    lwt = value;
                }
            } 
        }

        public MainWindow()
        {
            InitializeComponent();

            string user = Environment.GetEnvironmentVariable("USERPROFILE");
            string dir1 = @$"{user}\AppData\Local\Hinterland\TheLongDark";
            string dir2 = @$"{user}\AppData\Local\Hinterland\TheLongDarkSaves";

            BaseDir = new DirectoryInfo(dir1);

            if (Directory.Exists(dir2))
            {
                SavesDir = new DirectoryInfo(dir2);
                UpdateSavesList();
            }
            else SavesDir = Directory.CreateDirectory(dir2);

            this.Closing += (s, e) => AppLive = false;
            var wachTask = Waching();
        }

        private void UpdateSavesList()
        {
            Saves = SavesDir.GetDirectories().OrderByDescending(d => d.Name).ToList();
            var ls = Saves.Select(s => s.Name).ToList();
            Dispatcher.Invoke(() => SavesList.ItemsSource = ls);
        }

        private Task Waching()
        {
            return Task.Run(async () => 
            {
                LWT = BaseDir.LastWriteTime;

                while (AppLive)
                {
                    await Task.Delay(1000);
                    BaseDir.Refresh();
                    if (LWT != BaseDir.LastWriteTime)
                    {
                        LWT = BaseDir.LastWriteTime;
                        Dispatcher.Invoke(() => WacherGrid.Background = Brushes.Yellow);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => WacherGrid.Background = Brushes.Transparent);
                    }
                }
            });
        }

        private async void SaveShowSuccess()
        {
            Dispatcher.Invoke(() => SaveGrid.Background = Brushes.Lime);
            await Task.Delay(100);
            Dispatcher.Invoke(() => SaveGrid.Background = Brushes.Transparent);
            await Task.Delay(100);
            Dispatcher.Invoke(() => SaveGrid.Background = Brushes.Lime);
            await Task.Delay(100);
            Dispatcher.Invoke(() => SaveGrid.Background = Brushes.Transparent);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            Task.Run(() => 
            {
                var label = Dispatcher.Invoke(() => SaveLabel.Text);
                Dispatcher.Invoke(() => SaveLabel.Text = "");

                BaseDir.Refresh();
                LWT = BaseDir.LastWriteTime;

                string name = $"{DateTime.Now:yy.MM.dd-HH.mm.ss}";
                if (label != "") name += $" - {label}";

                Directory.CreateDirectory(@$"{SavesDir.FullName}\{name}");

                foreach (var f in BaseDir.GetFiles())
                    f.CopyTo(@$"{SavesDir.FullName}\{name}\{f.Name}");

                UpdateSavesList();
                SaveShowSuccess();
            });
        }

        private async void LoadShowSuccess()
        {
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Lime);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Transparent);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Lime);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Transparent);
        }
        private async void LoadShowError()
        {
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Red);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Transparent);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Red);
            await Task.Delay(100);
            Dispatcher.Invoke(() => LoadGrid.Background = Brushes.Transparent);
        }

        private void DeleteDirectory(DirectoryInfo dir)
        {
            foreach (var d in dir.GetDirectories()) DeleteDirectory(d);
            foreach (var f in dir.GetFiles()) f.Delete();
            dir.Delete();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var dir = Dispatcher.Invoke(() => (string)SavesList.SelectedItem);

                if (dir != null)
                {
                    DeleteDirectory(SavesDir.GetDirectories().Where(d => d.Name == dir).First());
                    UpdateSavesList();
                    LoadShowSuccess();
                }
                else
                {
                    LoadShowError();
                }
            });
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var dir = Dispatcher.Invoke(() => (string)SavesList.SelectedItem);

                if (dir != null)
                {
                    try
                    {
                        foreach (var f in BaseDir.GetFiles()) f.Delete();
                        var Dir = SavesDir.GetDirectories().Where(d => d.Name == dir).First();
                        foreach (var f in Dir.GetFiles()) f.CopyTo(@$"{BaseDir.FullName}\{f.Name}");

                        BaseDir.Refresh();
                        LWT = BaseDir.LastWriteTime;

                        LoadShowSuccess();
                    }
                    catch 
                    {
                        LoadShowError();
                    }
                }
                else
                {
                    LoadShowError();
                }
            });
        }
    }
}