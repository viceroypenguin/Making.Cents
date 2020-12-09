using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;

namespace Making.Cents.Wpf.Common.ViewModels
{
	public class LoadingViewModel : ViewModelBase
	{
		public string? Message { get; set; }
		public bool IsLoading { get; set; }

		public IDisposable Wait(string? message)
		{
			Message = message ?? "Loading...";
			IsLoading = true;
			return new LoadingDisposable(EndWait);
		}

		private void EndWait()
		{
			Message = default;
			IsLoading = false;
		}

		private class LoadingDisposable : IDisposable
		{
			private readonly Action _endWait;

			public LoadingDisposable(Action endWait)
			{
				_endWait = endWait;
			}

			public void Dispose() =>
				_endWait();
		}
	}
}
