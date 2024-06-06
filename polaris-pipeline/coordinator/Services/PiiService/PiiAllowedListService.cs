using System.Linq;

namespace coordinator.Services.PiiService
{
    public class PiiAllowedListService : IPiiAllowedListService
    {
        private readonly IPiiAllowedList _piiAllowedList;

        public PiiAllowedListService(IPiiAllowedList piiAllowedList)
        {
            _piiAllowedList = piiAllowedList;
        }
        public bool Contains(string word, string category)
        {
            var result = PiiAllowListLookup[word];
            return result.Contains(category);
        }

        private ILookup<string, string> PiiAllowListLookup =>
            _piiAllowedList.GetWords().ToLookup(x => x.Word, x => x.Category);
    }
}