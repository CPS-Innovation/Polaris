namespace Common.Dto.Request.DocumentManipulation
{
    public class DocumentChangesDto
    {
        public int PageIndex { get; set; }
        public string Operation { get; set; }
        public object Arg { get; set; }
    }
}