using Common.Configuration;
using Newtonsoft.Json;

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

		[JsonProperty("trackerUrl")]
		public string TrackerUrl
		{
			get => "/api/" + RestApi.GetCaseTrackerPath(_caseUrn, _caseId);
		}
	}
}

