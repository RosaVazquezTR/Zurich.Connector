using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zurich.Connector.Tests
{
    public class Utility
    {
		public static IConfiguration CreateConfiguration(string key, string value)
		{
			var myConfiguration = new Dictionary<string, string>
			{
				{key, value}
			};
			return new ConfigurationBuilder()
				.AddInMemoryCollection(myConfiguration)
				.Build();
		}

		public static IConfiguration CreateConfiguration(Dictionary<string, string> myConfiguration)
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(myConfiguration)
				.Build();
		}
	}
}
