using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Threading;

namespace Wpftest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //class Song
    //{
    //    public Song(string songName, string songPath) 
    //    {
    //        this.songName = songName;
    //        this.songPath = songPath;  
    //    }
    //    public string songName { get; set; }
    //    public string songPath { get; set; }
    //}

    public partial class MainWindow : Window
    {
        //private string file = string.Empty;
        bool isDragging = false;
        string currentTime = "00:00";
        string totalTime = "00:00";
        bool isPlay = false;
        double historyVolume = 50;
        int[] cards;

        private ViewModel VM = new ViewModel();

        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = VM;
            VM.Songs = getSongsFromJson();
            cards = GenerateIntArray(VM.Songs.Count);
            _MediaElement.MediaOpened += _MediaElement_MediaOpened;
            
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (s, e) =>
            {
                var ts = _MediaElement.Position;
                _seekBar.Value = ts.TotalSeconds;
                currentTime = TimeSpanFormat(ts);
                _timeTextBlock.Text = $"{currentTime}/{totalTime}";
            };
        }

        private void _MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_MediaElement.NaturalDuration.HasTimeSpan)
            {
                timer.Stop();
                timer.Start();
                string songPath = VM.Songs[cards[VM.SongIndex]].songPath;
                VM.MusicText = System.IO.Path.GetFileNameWithoutExtension(songPath);
                var ts = _MediaElement.NaturalDuration.TimeSpan;
                _seekBar.Maximum = _MediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                _seekBar.Value = 0;
                totalTime = TimeSpanFormat(_MediaElement.NaturalDuration.TimeSpan);
                _timeTextBlock.Text = $"{currentTime}/{totalTime}";
                _playIcon.Source = (DrawingImage)Application.Current.TryFindResource("PauseIcon");
                isPlay = true;
            }
        }

        private void Play_Stop_Media(object sender, RoutedEventArgs e)
        {
            if(isPlay == false)
            {
                _MediaElement.Play();//播放
                _playIcon.Source = (DrawingImage)Application.Current.TryFindResource("PauseIcon"); 
                isPlay = true;
            }
            else
            {
                _MediaElement.Pause();
                _playIcon.Source = (DrawingImage)Application.Current.TryFindResource("PlayIcon");
                isPlay = false;
            }
            
        }


        private void lastSong(object sender, RoutedEventArgs e)
        {
            if (_MediaElement.Source != null)
            {
                _MediaElement.Stop();//停止
                VM.SongIndex = (VM.SongIndex + VM.Songs.Count - 1) % VM.Songs.Count;
                string songPath = VM.Songs[cards[VM.SongIndex]].songPath;
                _MediaElement.Source = new Uri(songPath);
                _MediaElement.Play();//播放
            }
        }

        private void nextSong(object sender, RoutedEventArgs e)
        {
            if (_MediaElement.Source != null)
            {
                _MediaElement.Stop();//停止
                VM.SongIndex = (VM.SongIndex + 1) % VM.Songs.Count;
                string songPath = VM.Songs[cards[VM.SongIndex]].songPath;
                _MediaElement.Source = new Uri(songPath);
                _MediaElement.Play();//播放
            }
                
        }

        private void Volume_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_Slider.Value != 0)
            {
                historyVolume = _Slider.Value;
                _Slider.Value = 0;
            }
            else
            {
                _Slider.Value = historyVolume;
            }
        }

        private void _Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _MediaElement.Volume = _Slider.Value;//设置音量
            if (_Slider.Value == 0)
            {
                _volumeIcon.Source = (DrawingImage)Application.Current.TryFindResource("MuteIcon");
            }
            else
            {
                _volumeIcon.Source = (DrawingImage)Application.Current.TryFindResource("VolumeIcon");
            }
        }

        private void _MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (VM.PlayMode_ != PlayMode.Single)
            { 
                VM.SongIndex = (VM.SongIndex + 1) % VM.Songs.Count;
            }
            string songPath = VM.Songs[cards[VM.SongIndex]].songPath;
            _MediaElement.Stop();//停止
            _MediaElement.Source = new Uri(songPath);
            _MediaElement.Play();//播放
        }

        private void _SongsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var selectedItem = listBox.SelectedItem;
                var selectedValue = listBox.SelectedValue;
                if (selectedItem != null)
                {
                    // 这里处理双击事件
                    _MediaElement.Stop();//停止
                    _MediaElement.Source = new Uri(selectedValue.ToString());
                    _MediaElement.Play();//播放
                    for (int i = 0; i < VM.Songs.Count; i++)
                    {
                        VM.SongIndex = i;
                        string songPath = VM.Songs[cards[VM.SongIndex]].songPath;
                        if(songPath == selectedValue.ToString())
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void updateSongs(object sender, RoutedEventArgs e)
        {
            var path = VM.SongsDictionaryPath;
            if (path != null && Directory.Exists(path))
            {
                List<string> names = FindAllMp3Files(path);
                string outputPath = @".\songs.json";
                string jsonOutput = JsonConvert.SerializeObject(names, Formatting.Indented);
                // 将JSON字符串写入文件
                using (StreamWriter fileWriter = new StreamWriter(outputPath))
                {
                    fileWriter.Write(jsonOutput);
                }
                MessageBox.Show("更新完成");
                VM.Songs = getSongsFromJson();
                cards = GenerateIntArray(VM.Songs.Count);
                VM.MusicText = $"欢迎使用{VM.PlayerTitle}";
            }
            else
            {
                MessageBox.Show("请输入正确的文件夹路径");
            }
            VM.SongsDictionaryPath = "";
        }

        private List<string> FindAllMp3Files(string rootPath)
        {
            // 使用 List<string> 来收集所有 .mp3 文件的路径
            var mp3Paths = new List<string>();

            // 递归遍历文件夹
            void RecursiveSearch(string path)
            {
                // 获取当前文件夹中的所有文件
                var files = Directory.GetFiles(path, "*.mp3");
                foreach (var file in files)
                {
                    // 添加文件路径到列表中
                    mp3Paths.Add(file);
                }

                // 获取当前文件夹中的所有子文件夹
                var subDirs = Directory.GetDirectories(path);
                foreach (var dir in subDirs)
                {
                    // 对每个子文件夹递归调用此方法
                    RecursiveSearch(dir);
                }
            }

            // 从根目录开始递归
            RecursiveSearch(rootPath);

            // 将 List 转换为数组并返回
            return mp3Paths;
        }
        
        private ObservableCollection<DataModel.Song> getSongsFromJson()
        {
            var songs = new ObservableCollection<DataModel.Song>();
            string filePath = @".\songs.json";
            if (File.Exists(filePath))
            {
                using (StreamReader fileReader = new StreamReader(filePath))
                {
                    // 从文件中获取JSON字符串
                    string jsonContent = fileReader.ReadToEnd();

                    // 使用JsonConvert.DeserializeObject反序列化JSON字符串为User对象
                    var songsPath = JsonConvert.DeserializeObject<List<string>>(jsonContent);
                    if (songsPath != null)
                    {
                        foreach (string songPath in songsPath)
                        {
                            songs.Add(new DataModel.Song(System.IO.Path.GetFileNameWithoutExtension(songPath), songPath));
                        }
                    }

                };
            }
            else 
            {
                VM.MusicText = "请更新歌曲目录";
            }
            return songs;
        }

        private void _seekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_MediaElement.IsLoaded && !isDragging && _MediaElement.Source!=null)
            {
                // 如果 Slider 的值改变且不在拖动状态，则更新 MediaElement 的播放位置
                _MediaElement.Position = TimeSpan.FromSeconds(_seekBar.Value);
                currentTime = TimeSpanFormat(_MediaElement.Position);
                _timeTextBlock.Text = $"{currentTime}/{totalTime}";
            }
        }

        private void _seekBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void _seekBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            _MediaElement.Position = TimeSpan.FromSeconds(_seekBar.Value);
            currentTime = TimeSpanFormat(_MediaElement.Position);
            _timeTextBlock.Text = $"{currentTime}/{totalTime}";
        }

        private string TimeSpanFormat(TimeSpan duration)
        {
            string formattedDuration = string.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);
            return formattedDuration;
        }

        private int[] GenerateIntArray(int length)
        {
            int[] arr = new int[length];
            switch (VM.PlayMode_)
            {
                case PlayMode.Sequential:
                    for (int i = 0; i < length; i++)
                    {
                        arr[i] = i;
                    }
                    break;
                case PlayMode.Single:
                    for (int i = 0; i < length; i++)
                    {
                        arr[i] = i;
                    }
                    break;
                case PlayMode.Random:
                    for (int i = 0; i < length; i++)
                    {
                        arr[i] = i;
                    }
                    KnuthDurstenfeldShuffle(arr);
                    break;
            }
            
            return arr;
        }

        public void KnuthDurstenfeldShuffle(int[] arr)
        {
            //随机交换
            int currentIndex;
            int tempValue;
            Random rng = new Random();
            for (int i = 0; i < arr.Length; i++)
            {
                currentIndex = rng.Next(0, arr.Length - i);
                tempValue = arr[currentIndex];
                arr[currentIndex] = arr[arr.Length - 1 - i];
                arr[arr.Length - 1 - i] = tempValue;
            }
            int delta = cards[VM.SongIndex] - arr[VM.SongIndex] ;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (arr[i] + VM.Songs.Count + delta) % VM.Songs.Count;
            }
        }

        private void Change_Play_Mode(object sender, RoutedEventArgs e)
        {
            VM.PlayMode_ = (PlayMode)(((int)VM.PlayMode_ + 1) % 3);
            switch (VM.PlayMode_)
            {
                case PlayMode.Sequential:
                    _playModeIcon.Source = (DrawingImage)Application.Current.TryFindResource("Sequential");
                    VM.PlayModeToolTip = "列表循环";
                    break;
                case PlayMode.Single:
                    _playModeIcon.Source = (DrawingImage)Application.Current.TryFindResource("Single");
                    VM.PlayModeToolTip = "单曲循环";
                    break;
                case PlayMode.Random:
                    _playModeIcon.Source = (DrawingImage)Application.Current.TryFindResource("Random");
                    VM.PlayModeToolTip = "随机播放";
                    break;
            }
            if (VM.Songs.Count > 0) { 
                cards = GenerateIntArray(VM.Songs.Count);
            }
        }
    }
}