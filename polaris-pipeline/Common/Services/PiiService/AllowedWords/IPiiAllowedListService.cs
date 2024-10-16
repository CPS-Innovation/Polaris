namespace Common.Services.PiiService.AllowedWords
{
    public interface IPiiAllowedListService
    {
        bool Contains(string word, string category);
    }
}