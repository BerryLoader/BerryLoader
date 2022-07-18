using BepInEx;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BerryLoaderNS
{
	public static class DependencyHelper
	{
		public static bool IsDependentOnBerryLoader(KeyValuePair<string, PluginInfo> x)
		{
			return x.Value?.Instance && x.Value.Dependencies.Any(i => i.DependencyGUID == "BerryLoader");
		}
	}
}