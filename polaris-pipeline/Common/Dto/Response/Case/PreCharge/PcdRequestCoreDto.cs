using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.Case.PreCharge
{
  public class PcdRequestCoreDto
  {
    public int Id { get; set; }

    public string DecisionRequiredBy { get; set; }

    public string DecisionRequested { get; set; }

    public PresentationFlagsDto PresentationFlags { get; set; }
  }
}