using System.ComponentModel.DataAnnotations;

namespace Common.tests.Wrappers
{
	public class StubRequest
	{
		[Required]
		public string StubString { get; set; } = string.Empty;
	}
}

