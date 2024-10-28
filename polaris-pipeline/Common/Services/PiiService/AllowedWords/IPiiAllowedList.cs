using System.Collections.Generic;

namespace Common.Services.PiiService.AllowedWords
{
    public interface IPiiAllowedList
    {
        List<PiiAllowedWord> GetWords();
    }
}