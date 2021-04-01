using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zurich.Connector.Web
{
	public static class BootStrapper
	{
		public static void Bootstrap(IServiceCollection services)
		{
			BootstrapDependencies(services);
		}

		private static void BootstrapDependencies(IServiceCollection services)
		{
			// Add application services.
			App.Bootstrapper.Bootstrap(services);
		}
	}
}
