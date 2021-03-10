using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Contracts
{
	public interface IMainWindowTab
	{
		string Title { get; }

		bool CanClose();
		void Close();
	}
}
