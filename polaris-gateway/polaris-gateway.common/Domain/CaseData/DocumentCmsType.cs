namespace PolarisGateway.Domain.CaseData
{
    public class DocumentCmsType
    {
        public DocumentCmsType(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public readonly static DocumentCmsType EmptyDocumentCmsType = new DocumentCmsType(-1, "Unknown", "Unknown");
    }
}