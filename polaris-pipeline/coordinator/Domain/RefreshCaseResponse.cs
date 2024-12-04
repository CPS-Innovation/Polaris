using Common.Configuration;
using System.Text.Json.Serialization;

namespace coordinator.Domain
{
	public class RefreshCaseResponse
	{
		private readonly string _caseUrn;
		private readonly int _caseId;

		public RefreshCaseResponse(string caseUrn, int caseId)
		{
			_caseUrn = caseUrn;
			_caseId = caseId;
		}

		[JsonPropertyName("trackerUrl")]
		public string TrackerUrl
		{
			get => "/api/" + RestApi.GetCaseTrackerPath(_caseUrn, _caseId);
		}
	}
}

