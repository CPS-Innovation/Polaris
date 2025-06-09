namespace Common.Dto.Response.Case
{
    public class CaseLockingDto
    {
        public string Application { get; set; }
        public string BySurname { get; set; }
        public string ByFirstNames { get; set; }
        public string Locked { get; set; }
        public string Since { get; set; }
    }
}
