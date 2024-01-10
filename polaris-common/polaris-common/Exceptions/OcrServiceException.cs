namespace polaris_common.Domain.Exceptions
{
	[Serializable]
	public class OcrServiceException : Exception
	{
		public OcrServiceException(string message) : base(message)
		{
		}
	}
}

