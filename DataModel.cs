using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpftest
{
    internal enum PlayMode
    {
        Sequential = 0,
        Single,
        Random,
    }

    public static class GlobalValue
    {
        public static readonly string PlayerTitle = "阿零音乐v0.1";
    }

    internal class DataModel : ObservableObject
    {

        //歌曲类型，用于列表展示
        public class Song
        {
            public Song(string songName, string songPath)
            {
                this.songName = songName;
                this.songPath = songPath;
            }
            public string songName { get; set; }
            public string songPath { get; set; }
        }
        //歌名和路径的集合
        private ObservableCollection<Song> _Songs;
        public ObservableCollection<Song> Songs
        { 
            get { return _Songs; } 
            set { _Songs = value; RaisePropertyChanged(); }
        }

        //上方展示歌名对应的TextBlock
        private string _MusicText;

        public string MusicText
        {
            get { return _MusicText; }
            set { _MusicText = value; RaisePropertyChanged(); }
        }

        //正在播放的歌曲的下标
        private int _SongIndex;

        public int SongIndex
        {
            get { return _SongIndex; }
            set { _SongIndex = value; }
        }

        //歌曲文件夹路径
        private string _SongsDictionaryPath;

        public string SongsDictionaryPath
        {
            get { return _SongsDictionaryPath; }
            set { _SongsDictionaryPath = value; RaisePropertyChanged(); }
        }

        //歌曲播放模式
        private PlayMode _PlayMode_;

        public PlayMode PlayMode_
        {
            get { return _PlayMode_; }
            set { _PlayMode_ = value; }
        }

        private string _PlayModeToolTip;

        //切换播放模式按钮的ToolTip
        public string  PlayModeToolTip
        {
            get { return _PlayModeToolTip; }
            set { _PlayModeToolTip = value; }
        }

        public DataModel()
        {
            MusicText = $"欢迎使用{GlobalValue.PlayerTitle}";
            PlayMode_ = PlayMode.Sequential;
            PlayModeToolTip = "列表循环";
        }
    }
}
