namespace pdf_redactor.Domain.Exceptions
{
	[Serializable]
	public class AsposeLicenseException : Exception
	{
		public AsposeLicenseException(string message) :
			base($"Failed to set Aspose License: {message}.")
		{
		}
	}
}

