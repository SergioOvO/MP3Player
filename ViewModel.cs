using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpftest
{
    internal class ViewModel : ObservableObject
    {
		private DataModel _DataModel = new DataModel();

		public ObservableCollection<DataModel.Song> Songs
		{
			get { return _DataModel.Songs; }
			set { _DataModel.Songs = value; RaisePropertyChanged(); }
		}

		public string MusicText
		{
			get { return _DataModel.MusicText; }
			set { _DataModel.MusicText = value; RaisePropertyChanged(); }
		}

		public int SongIndex
		{ 
			get { return _DataModel.SongIndex; } 
			set { _DataModel.SongIndex = value; RaisePropertyChanged(); }
		}

		public string SongsDictionaryPath
		{
			get { return _DataModel.SongsDictionaryPath; }
			set { _DataModel.SongsDictionaryPath = value; RaisePropertyChanged(); }
		}

		public PlayMode PlayMode_
		{
			get { return _DataModel.PlayMode_; }
			set { _DataModel.PlayMode_ = value; }
		}

		public string PlayModeToolTip
		{
			get { return _DataModel.PlayModeToolTip; }
			set { _DataModel.PlayModeToolTip = value; RaisePropertyChanged(); }
		}

		public string PlayerTitle
		{
			get { return GlobalValue.PlayerTitle; }
		}
	}
}
