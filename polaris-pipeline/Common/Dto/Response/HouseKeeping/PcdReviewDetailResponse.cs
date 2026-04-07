using Cps.MasterDataService.Infrastructure.ApiClient;

namespace Common.Dto.Response.HouseKeeping;

public class PcdReviewDetailResponse
{
    public PreChargeDecisionAnalysisOutcome PreChargeDecisionAnalysisOutcome { get; set; }
    public PreChargeDecisionOutcome PreChargeDecisionOutcome { get; set; }
}
