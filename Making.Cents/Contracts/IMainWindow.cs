using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.Common.Contracts
{
	public interface IMainWindow
	{
		void NavigateTab(IMainWindowTab tab);
	}
}
