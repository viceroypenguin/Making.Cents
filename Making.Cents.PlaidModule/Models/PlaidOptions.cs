using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Making.Cents.PlaidModule.Models
{
	public class PlaidOptions
	{
		public Dictionary<string, string> AccessTokens { get; init; } =
			new Dictionary<string, string>();
		public string PlaidUserId { get; init; } = null!;
	}
}
