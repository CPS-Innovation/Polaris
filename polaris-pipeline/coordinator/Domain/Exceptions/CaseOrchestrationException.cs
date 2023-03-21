using System;
namespace coordinator.Domain.Exceptions
{
	[Serializable]
	public class CaseOrchestrationException : Exception
	{
		public CaseOrchestrationException(string message) : base(message)
		{
		}
	}
}

