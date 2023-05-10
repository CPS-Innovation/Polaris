using System;

namespace Common.Health.Status
{
	public class AssemblyStatus
	{
		public string Name { get; set; }
		
		public string Version { get; set; }

		public DateTime LastBuilt { get; set; }
	}
}
