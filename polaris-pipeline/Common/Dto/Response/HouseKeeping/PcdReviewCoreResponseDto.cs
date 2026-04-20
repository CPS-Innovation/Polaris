using Cps.MasterDataService.Infrastructure.ApiClient;
using Common.Enums;

namespace Common.Dto.Response.HouseKeeping;

/// <summary>
/// Data Transfer Object for PCD Review Core response.
/// </summary>
public class PcdReviewCoreResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the PCD Review Core event.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the type of PCD Review Core event.
    /// </summary>
    public PcdReviewCoreType Type { get; set; }

    /// <summary>
    /// Gets or sets the date of the PCD Review Core event.
    /// </summary>
    public string Date { get; set; }
}
