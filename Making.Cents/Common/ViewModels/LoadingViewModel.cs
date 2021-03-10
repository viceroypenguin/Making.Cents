using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace Making.Cents.Common.ViewModels
{
	public class LoadingViewModel : ViewModelBase
	{
		public string? Message { get; set; }
		public bool IsLoading { get; set; }
		public bool Cancellable { get; private set; }

		public bool IsIndeterminate => true;
		public int Progress => 0;
		public string ProgressText => string.Empty;

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

		[Command]
		public void Cancel() { }
	}
}
