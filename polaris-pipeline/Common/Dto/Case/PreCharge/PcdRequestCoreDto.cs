using Common.Dto.FeatureFlags;

namespace Common.Dto.Case.PreCharge
{
  public class PcdRequestCoreDto
  {
    public int Id { get; set; }

    public string DecisionRequiredBy { get; set; }

    public string DecisionRequested { get; set; }

    public PresentationFlagsDto PresentationFlags { get; set; }
  }
}