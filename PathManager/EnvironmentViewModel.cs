using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public EnvironmentViewModel()
		{
			RefreshCommand = new ReactiveCommand();
			RefreshCommand.Subscribe(_ => Refresh());
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

		public ReactiveCommand RefreshCommand
		{
			get;
			private set;
		}

		private static IEnumerable<PathViewModel> GetPaths(EnvironmentVariableTarget mode, SettingType settingType)
		{
			string variable = Environment.GetEnvironmentVariable(PathVariable, mode);
			if (variable == null)
			{
				return Enumerable.Empty<PathViewModel>();
			}

			return variable
				.Split(Path.PathSeparator)
				.Select(path => new PathViewModel
				{
					SettingType = settingType,
					Path = path,
					IsChanged = false
				});
		}

		private void Refresh()
		{
			var userPaths = GetPaths(EnvironmentVariableTarget.User, SettingType.User);
			var systemPaths = GetPaths(EnvironmentVariableTarget.Machine, SettingType.System);

			Paths = new ObservableCollection<PathViewModel>(userPaths.Concat(systemPaths));
		}
	}
}
