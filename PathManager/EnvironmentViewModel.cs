using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace PathManager
{
	public class EnvironmentViewModel : ReactiveObject
	{
		private const string PathVariable = "PATH";

		private ObservableCollection<PathViewModel> _paths;
		private int _pathSize;
		private int _pathLimit;

		public EnvironmentViewModel()
		{
			RefreshCommand = new ReactiveCommand();
			RefreshCommand.Subscribe(_ => Refresh());
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

		private static IObservable<int> GetPathLimit()
		{
			var startInfo = new ProcessStartInfo("wmic", "qfe get hotfixid")
			{
				UseShellExecute = false,
				RedirectStandardOutput = true
			};

			var process = Process.Start(startInfo);

			return Observable.FromAsync(process.StandardOutput.ReadLineAsync)
				.Any()
				.Select(found => found ? 4095 : 2047); // Check http://support.microsoft.com/kb/2685893 for these constants.
		}

		private void Refresh()
		{
			int size = 0;
			var userPaths = GetPaths(EnvironmentVariableTarget.User, SettingType.User, ref size);
			var systemPaths = GetPaths(EnvironmentVariableTarget.Machine, SettingType.System, ref size);

			Paths = new ObservableCollection<PathViewModel>(userPaths.Concat(systemPaths));
			PathSize = size;
			GetPathLimit().Subscribe(limit => PathLimit = limit);
		}
	}
}
