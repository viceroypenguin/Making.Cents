using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Wpf.Common.Utilities
{
	public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
	{
		#region Constructors
		public ObservableCollection() { }

		public ObservableCollection(List<T> list)
			: base(list) { }

		public ObservableCollection(IEnumerable<T> collection)
			: base(collection) { }
		#endregion

		#region Support
		private void CopyFrom(IEnumerable<T> collection)
		{
			if (collection == null)
				return;
			var items = this.Items;
			foreach (var t in collection)
				items.Add(t);
		}

		private void OnPropertyChanged(string propertyName) =>
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

		private void OnCollectionReset() =>
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		#endregion

		#region New Methods
		public void AddRange(IEnumerable<T> collection)
		{
			CheckReentrancy();

			CopyFrom(collection);

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionReset();
		}

		public void Replace(IEnumerable<T> collection)
		{
			CheckReentrancy();

			ClearItems();
			CopyFrom(collection);

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionReset();
		}

		public void RemoveAll(Predicate<T> match)
		{
			CheckReentrancy();

			if (Items is not List<T> items)
				throw new InvalidOperationException("Internal assumption failed. Collection<T>.Items is no longer a List<T>.");

			var cnt = items.RemoveAll(match);

			if (cnt != 0)
			{
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnCollectionReset();
			}
		}
		#endregion
	}
}
