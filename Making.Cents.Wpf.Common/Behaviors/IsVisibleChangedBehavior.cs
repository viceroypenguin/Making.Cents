using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm.UI.Interactivity;

namespace Making.Cents.Wpf.Common.Behaviors
{
	public partial class IsVisibleChangedBehavior : Behavior<UserControl>
	{
		public static readonly DependencyProperty CommandProperty =
			Gen.Command<ICommand>();

		private ICommand? _command;
		private static void CommandPropertyChanged(
			IsVisibleChangedBehavior self,
			DependencyPropertyChangedEventArgs e)
		{
			self._command = e.NewValue as ICommand;
			self.Update();
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
			Update();
		}

		protected override void OnDetaching()
		{
			AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
			base.OnDetaching();
		}

		private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) =>
			Update((bool)e.NewValue);

		private void Update() =>
			Update(AssociatedObject.IsVisible);

		private bool _currentStatus = false;
		private void Update(bool newValue)
		{
			if (newValue != _currentStatus && _command != null)
			{
				_command.Execute(newValue);
				_currentStatus = newValue;
			}
		}
	}
}
