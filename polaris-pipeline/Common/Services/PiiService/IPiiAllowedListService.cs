namespace Common.Services.PiiService
{
    public interface IPiiAllowedListService
    {
        bool Contains(string word, string category);
    }
}