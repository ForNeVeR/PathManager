using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace PathManager
{
	public class PathViewModel : ReactiveObject
	{
		private string _path;
		private bool _changed;
		private SettingType _settingType;
		private string _changedText;

		public PathViewModel()
		{
			this.ObservableForProperty(t => t.Path)
				.Subscribe(c => { c.Sender.IsChanged = true; });

			this.ObservableForProperty(t => t.IsChanged)
				.Subscribe(c => { c.Sender.ChangedText = c.Value ? "(changed)" : "(not changed)"; });
		}

		public bool IsChanged
		{
			get { return _changed; }
			set { this.RaiseAndSetIfChanged(ref _changed, value); }
		}

		public string Path
		{
			get { return _path; }
			set { this.RaiseAndSetIfChanged(ref _path, value); }
		}

		public SettingType SettingType
		{
			get { return _settingType; }
			set { this.RaiseAndSetIfChanged(ref _settingType, value); }
		}

		public string ChangedText
		{
			get { return _changedText; }
			private set { this.RaiseAndSetIfChanged(ref _changedText, value); }
		}
	}
}
