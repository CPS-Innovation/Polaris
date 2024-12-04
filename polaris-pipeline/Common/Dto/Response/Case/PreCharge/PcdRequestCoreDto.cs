using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.Case.PreCharge
{
  public class PcdRequestCoreDto
  {
        public PcdRequestCoreDto()
        {
        }

        public PcdRequestCoreDto(int id, string decisionRequiredBy, string decisionRequested, PresentationFlagsDto presentationFlags)
        {
            Id = id;
            DecisionRequiredBy = decisionRequiredBy;
            DecisionRequested = decisionRequested;
            PresentationFlags = presentationFlags;
        }

        public int Id { get; set; }

    public string DecisionRequiredBy { get; set; }

    public string DecisionRequested { get; set; }

    public PresentationFlagsDto PresentationFlags { get; set; }
  }
}