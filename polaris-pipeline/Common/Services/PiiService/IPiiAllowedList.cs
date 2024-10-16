using System.Collections.Generic;

namespace Common.Services.PiiService
{
    public interface IPiiAllowedList
    {
        List<PiiAllowedWord> GetWords();
    }
}