using System;
namespace coordinator.Domain.Exceptions
{
	[Serializable]
	public class CoordinatorOrchestrationException : Exception
	{
		public CoordinatorOrchestrationException(string message) : base(message)
		{
		}
	}
}

