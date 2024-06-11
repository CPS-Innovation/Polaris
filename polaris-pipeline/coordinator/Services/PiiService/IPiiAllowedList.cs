using System.Collections.Generic;

namespace coordinator.Services.PiiService
{
    public interface IPiiAllowedList
    {
        List<PiiAllowedWord> GetWords();
    }
}