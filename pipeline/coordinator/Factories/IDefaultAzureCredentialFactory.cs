using Azure.Identity;

namespace coordinator.Factories
{
	public interface IDefaultAzureCredentialFactory
	{
		DefaultAzureCredential Create();
	}
}

