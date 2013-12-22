using Microsoft.Win32;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace PathManager
{
	public class EnvironmentViewModel : ReactiveObject
	{
		private const string PathVariable = "PATH";

		private readonly Lazy<Task<int>> _pathLimitSource = new Lazy<Task<int>>(GetPathLimit);

		private ObservableCollection<PathViewModel> _paths;
		private int _pathSize;
		private int _pathLimit;

		public EnvironmentViewModel()
		{
			RefreshCommand = new ReactiveCommand();
			var settingsChanged = Observable.FromEventPattern<UserPreferenceChangedEventHandler, UserPreferenceChangedEventArgs>(
				x => SystemEvents.UserPreferenceChanged += x,
				x => SystemEvents.UserPreferenceChanged -= x);

			RefreshCommand.Merge(settingsChanged).Subscribe(_ => Refresh());
		}

		public ReactiveCommand RefreshCommand
		{
			get;
			private set;
		}

		public ObservableCollection<PathViewModel> Paths
		{
			get
			{
				return _paths;
			}
			private set
			{
				this.RaiseAndSetIfChanged(ref _paths, value);
			}
		}

		public int PathSize
		{
			get
			{
				return _pathSize;
			}
			set
			{
				this.RaiseAndSetIfChanged(ref _pathSize, value);
			}
		}

		public int PathLimit
		{
			get
			{
				return _pathLimit;
			}
			set
			{
				this.RaiseAndSetIfChanged(ref _pathLimit, value);
			}
		}

		private static IEnumerable<PathViewModel> GetPaths(
			EnvironmentVariableTarget mode,
			SettingType settingType,
			ref int size)
		{
			string variable = Environment.GetEnvironmentVariable(PathVariable, mode);
			if (variable == null)
			{
				return Enumerable.Empty<PathViewModel>();
			}

			size += variable.Length;

			return variable
				.Split(Path.PathSeparator)
				.Select(path => new PathViewModel
				{
					SettingType = settingType,
					Path = path,
					IsChanged = false
				});
		}

		private static Task<int> GetPathLimit()
		{
            // Check http://support.microsoft.com/kb/2685893 for these constants.
            return Task.Factory.StartNew(() => Wmi.IsUpdateInstalled("KB2685893") ? 4095 : 2047);
		}

		private void Refresh()
		{
			int size = 0;
			var userPaths = GetPaths(EnvironmentVariableTarget.User, SettingType.User, ref size);
			var systemPaths = GetPaths(EnvironmentVariableTarget.Machine, SettingType.System, ref size);

			Paths = new ObservableCollection<PathViewModel>(userPaths.Concat(systemPaths));
			PathSize = size;

			// PathLimit cannot be changed without system reboot, so we currently may refresh it only once a session.
			Observable.FromAsync(() => _pathLimitSource.Value).Subscribe(limit => PathLimit = limit);
		}
	}
}
