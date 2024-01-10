namespace polaris_common.Health.Status
{
	public class AssemblyStatus
	{
		public string Name { get; set; }
		
		public string BuildVersion { get; set; }

		public string SourceVersion { get; set; }

		public DateTime LastBuilt { get; set; }
	}
}
