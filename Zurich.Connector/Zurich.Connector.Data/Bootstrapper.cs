using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zurich.Connector.Data
{
	public static class Bootstrapper
	{
		public static void Bootstrap(IServiceCollection services)
		{
			BootstrapDependencies(services);
		}

		public static void BootstrapDependencies(IServiceCollection services)
		{

		}

	}
}
