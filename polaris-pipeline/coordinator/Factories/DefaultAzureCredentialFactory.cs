using Azure.Identity;

namespace coordinator.Factories
{
	public class DefaultAzureCredentialFactory : IDefaultAzureCredentialFactory
	{
		public DefaultAzureCredential Create()
		{
			return new DefaultAzureCredential();
		}
	}
}