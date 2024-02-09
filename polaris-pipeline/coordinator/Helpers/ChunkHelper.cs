using System.Collections.Generic;

namespace coordinator.Helpers.ChunkHelper
{
    public static class ChunkHelper
    {
        public static List<List<string>> ChunkStringListByMaxCharacterCount(List<string> stringList, int maxCharacter)
        {
            var chunkedStringLists = new List<List<string>>();
            var currentChunk = new List<string>();
            var currentChunkLength = 0;

            foreach (var stringItem in stringList)
            {
                var stringLength = stringItem.Length;

                if (currentChunkLength + stringLength > maxCharacter)
                {
                    // add the current chunk to the list of chunks and start a new chunk
                    chunkedStringLists.Add(currentChunk);
                    currentChunk = new List<string>();
                    currentChunkLength = 0;
                }

                currentChunk.Add(stringItem);
                currentChunkLength += stringLength;
            }

            if (currentChunk.Count > 0)
            {
                chunkedStringLists.Add(currentChunk);
            }

            return chunkedStringLists;
        }
    }
}
