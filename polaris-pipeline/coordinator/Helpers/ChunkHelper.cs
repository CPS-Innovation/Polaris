using System.Collections.Generic;
using System.Text;

namespace coordinator.Helpers;

public static class ChunkHelper
{
    public static List<string> ChunkStringListByMaxCharacterCount(List<string> stringList, int maxCharacter)
    {
        var chunkedStrings = new List<string>();
        var stringBuilder = new StringBuilder();
        var currentChunkLength = 0;

        foreach (var stringItem in stringList)
        {
            var stringLength = stringItem.Length;

            if (currentChunkLength + stringLength + 5 > maxCharacter) // add the length of ", " and "[]"
            {
                chunkedStrings.Add($"[{stringBuilder}]");
                stringBuilder.Clear();
                currentChunkLength = 0;
            }

            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(", ");
                currentChunkLength += 2;
            }

            stringBuilder.Append($"\"{stringItem}\"");
            currentChunkLength += stringLength + 2; // add the length of the double quotes
        }

        if (stringBuilder.Length > 0)
        {
            chunkedStrings.Add($"[{stringBuilder}]");
        }

        return chunkedStrings;
    }
}
