namespace DdeiClient.Domain.Response;

public class MdsMaterialTypeListResponse
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string LongDescription { get; set; }
    public string Classification { get; set; }
    public string IsCaseMaterial { get; set; }
    public IEnumerable<AssociatedDispatchDocumentType> AssociatedDispatchDocumentTypes { get; set; }
    public bool PreventCheckOutAfterDispatch { get; set; }
    public bool PreventCheckOutOfClassisDocument { get; set; }
    public string ListOrder { get; set; }
    public string Selectable { get; set; }
    public string AddAsUsedOrUnused { get; set; }
    public int? ListOrderValue { get; set; }
}