namespace Common.Dto.Case
{
    public class DocumentCmsTypeDto
    {
        public DocumentCmsTypeDto(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public readonly static DocumentCmsTypeDto EmptyDocumentCmsType = new DocumentCmsTypeDto(-1, "Unknown", "Unknown");
    }
}