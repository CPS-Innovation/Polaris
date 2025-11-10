namespace Ddei.Domain.Response
{
    public class CaseLockingDto
    {
        public string Application { get; set; }
        public string BySurname { get; set; }
        public string ByFirstNames { get; set; }
        public bool Locked { get; set; }
        public string Since { get; set; }
    }
}